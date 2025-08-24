using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace DuoAuthCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public IActionResult GetConfig()
        {
            var duoConfig = _configuration.GetSection("Duo");
            return Ok(new {
                ClientId = duoConfig["ClientId"],
                ClientIdLength = duoConfig["ClientId"]?.Length,
                ApiHost = duoConfig["ApiHost"],
                HasClientId = !string.IsNullOrEmpty(duoConfig["ClientId"])
            });
        }

        /// <summary>
        /// Vérifie si l'utilisateur est authentifié via Duo
        /// Appelé par l'application ASP Classic pour vérifier l'état d'authentification
        /// </summary>
        [HttpGet("check")]
        public IActionResult CheckAuth([FromQuery] string username)
        {
            try
            {
                var session = _httpContextAccessor.HttpContext.Session;
                var sessionUsername = session.GetString("_Username");
                var sessionState = session.GetString("_State");
                var duoAuthenticated = session.GetString("_DuoAuthenticated");

                // Vérifier si l'utilisateur est authentifié via Duo
                if (!string.IsNullOrEmpty(duoAuthenticated) && 
                    !string.IsNullOrEmpty(sessionUsername) && 
                    sessionUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new
                    {
                        Authenticated = true,
                        Username = sessionUsername,
                        Timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    Authenticated = false,
                    Username = username,
                    Message = "Utilisateur non authentifié via Duo"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Erreur lors de la vérification d'authentification",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Démarre le processus d'authentification Duo pour un utilisateur
        /// Appelé par l'application ASP Classic pour initier l'authentification MFA
        /// </summary>
        [HttpPost("init")]
        public IActionResult InitAuth([FromBody] InitAuthRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username))
                {
                    return BadRequest(new { Error = "Nom d'utilisateur requis" });
                }

                var session = _httpContextAccessor.HttpContext.Session;
                
                // Stocker le nom d'utilisateur en session
                session.SetString("_Username", request.Username);
                session.SetString("_LegacyApp", "true");

                // Générer un état unique pour cette session
                var state = Guid.NewGuid().ToString();
                session.SetString("_State", state);

                return Ok(new
                {
                    Success = true,
                    Username = request.Username,
                    State = state,
                    Message = "Authentification initialisée, rediriger vers Duo"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Erreur lors de l'initialisation de l'authentification",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Génère l'URL de redirection vers Duo pour l'authentification
        /// Appelé par l'application ASP Classic après /api/auth/init
        /// </summary>
        [HttpGet("duo-url")]
        public IActionResult GetDuoUrl([FromQuery] string username, [FromQuery] string state)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(state))
                {
                    return BadRequest(new { Error = "Nom d'utilisateur et état requis" });
                }

                var session = _httpContextAccessor.HttpContext.Session;
                var sessionUsername = session.GetString("_Username");
                var sessionState = session.GetString("_State");

                // Vérifier que l'état et l'utilisateur correspondent
                if (sessionUsername != username || sessionState != state)
                {
                    return BadRequest(new { Error = "État ou utilisateur invalide" });
                }

                // Construire l'URL de redirection vers Duo
                var duoUrl = $"https://api-{_configuration["Duo:ApiHost"]}/oauth/v1/authorize?" +
                            $"client_id={Uri.EscapeDataString(_configuration["Duo:ClientId"])}&" +
                            $"redirect_uri={Uri.EscapeDataString(_configuration["Duo:RedirectUri"])}&" +
                            $"response_type=code&" +
                            $"scope=openid&" +
                            $"state={Uri.EscapeDataString(state)}";

                return Ok(new
                {
                    Success = true,
                    DuoUrl = duoUrl,
                    Username = username,
                    State = state,
                    Message = "URL de redirection vers Duo générée"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Erreur lors de la génération de l'URL Duo",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Déconnecte l'utilisateur
        /// Appelé par l'application ASP Classic pour la déconnexion
        /// </summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext.Session;
                session.Clear();

                return Ok(new
                {
                    Success = true,
                    Message = "Utilisateur déconnecté"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Erreur lors de la déconnexion",
                    Details = ex.Message
                });
            }
        }
    }

    public class InitAuthRequest
    {
        public string Username { get; set; }
    }
} 