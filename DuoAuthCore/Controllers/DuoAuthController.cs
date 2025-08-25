using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DuoAuthCore.Providers;
using DuoAuthCore.Services;

namespace DuoAuthCore.Controllers
{
    /// <summary>
    /// Contrôleur principal pour l'authentification Duo
    /// Utilise directement les classes officielles de Duo
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DuoAuthController : ControllerBase
    {
        private readonly IDuoClientProvider _duoClientProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DuoAuthController> _logger;
        private readonly TempAuthStorage _tempAuthStorage;

        public DuoAuthController(
            IDuoClientProvider duoClientProvider, 
            IHttpContextAccessor httpContextAccessor, 
            IConfiguration configuration,
            ILogger<DuoAuthController> logger,
            TempAuthStorage tempAuthStorage)
        {
            _duoClientProvider = duoClientProvider;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
            _tempAuthStorage = tempAuthStorage;
        }

        /// <summary>
        /// Endpoint pour l'authentification Duo depuis ASP Classic
        /// Redirige directement vers Duo après avoir initialisé la session
        /// </summary>
        /// <param name="username">Nom d'utilisateur</param>
        /// <param name="returnUrl">URL de retour (optionnel)</param>
        /// <returns>Redirection vers Duo ou erreur</returns>
        [HttpGet("duo-auth")]
        public IActionResult DuoAuth([FromQuery] string username, [FromQuery] string? returnUrl = null)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("Tentative d'authentification sans nom d'utilisateur");
                    return BadRequest(new { error = "Nom d'utilisateur requis" });
                }

                var session = _httpContextAccessor.HttpContext.Session;
                
                // Stocker le nom d'utilisateur en session
                session.SetString("_Username", username);
                session.SetString("_LegacyApp", "true");

                // Générer un état unique pour cette session
                var state = Guid.NewGuid().ToString();
                session.SetString("_State", state);

                // Stocker les données temporaires
                var authData = new AuthData
                {
                    Username = username,
                    State = state,
                    ReturnUrl = returnUrl ?? string.Empty
                };
                _tempAuthStorage.Store(state, authData);

                // Utiliser la méthode officielle de Duo pour créer l'URL d'authentification
                var duoClient = _duoClientProvider.GetDuoClient();
                var authUrl = duoClient.GenerateAuthUri(username, state);
                
                _logger.LogInformation("Redirection vers Duo pour l'utilisateur: {Username}", username);
                _logger.LogDebug("URL Duo: {AuthUrl}", authUrl);

                // Rediriger directement vers Duo
                return Redirect(authUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'initialisation de l'authentification Duo");
                return StatusCode(500, new { error = "Erreur lors de l'initialisation de l'authentification Duo" });
            }
        }

        /// <summary>
        /// Endpoint de callback après authentification Duo
        /// Utilise directement la logique officielle de Duo
        /// </summary>
        /// <param name="code">Code d'autorisation Duo</param>
        /// <param name="state">État de la session</param>
        /// <returns>Redirection vers l'application ASP Classic</returns>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                _logger.LogWarning("Callback invalide: code ou state manquant");
                return BadRequest(new { error = "Code ou state manquant" });
            }

            var sessionUsername = HttpContext.Session.GetString("_Username");
            if (string.IsNullOrEmpty(sessionUsername))
            {
                _logger.LogWarning("Session invalide: username introuvable");
                return BadRequest(new { error = "Session invalide, username introuvable" });
            }

            try
            {
                // Utiliser directement la méthode officielle de Duo
                var duoClient = _duoClientProvider.GetDuoClient();
                var idToken = await duoClient.ExchangeAuthorizationCodeFor2faResult(code, sessionUsername);

                if (idToken == null)
                {
                    _logger.LogError("Échec de l'échange du code d'autorisation");
                    return BadRequest(new { error = "Échec de l'échange du code d'autorisation" });
                }

                // Vérifier le résultat MFA via la classe officielle AuthResult
                if (idToken.AuthResult?.Result != "allow")
                {
                    _logger.LogWarning("Authentification Duo refusée pour l'utilisateur: {Username}, Status: {Status}", 
                        sessionUsername, idToken.AuthResult?.StatusMsg);
                    return Unauthorized(new { error = "Authentification Duo refusée" });
                }

                _logger.LogInformation("Authentification Duo réussie pour l'utilisateur: {Username}", sessionUsername);

                // Récupérer l'URL de retour depuis le stockage temporaire
                var authData = _tempAuthStorage.Retrieve(state);
                var returnUrl = authData?.ReturnUrl ?? "http://localhost/asp/duo_callback.asp";

                // Construire l'URL de redirection avec le token JWT complet
                // Le token JWT est accessible via idToken.IdToken si nécessaire
                var redirectUrl = $"{returnUrl}?username={Uri.EscapeDataString(sessionUsername)}&auth_result=success&token={Uri.EscapeDataString(idToken.ToString())}";
                
                // Nettoyer les données temporaires
                _tempAuthStorage.Remove(state);

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur durant le callback Duo");
                return StatusCode(500, new { error = "Erreur durant le callback Duo" });
            }
        }

        /// <summary>
        /// Endpoint pour valider un token JWT en utilisant les utilitaires officiels de Duo
        /// </summary>
        /// <param name="token">Token JWT à valider</param>
        /// <returns>Résultat de la validation</returns>
        [HttpGet("validate-token")]
        public IActionResult ValidateToken([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { error = "Token requis" });
                }

                // Utiliser directement les utilitaires officiels de Duo pour décoder le token
                var decodedToken = DuoAuthCore.Utils.DecodeToken(token);
                
                if (decodedToken != null)
                {
                    return Ok(new
                    {
                        success = true,
                        username = decodedToken.Username,
                        expiresAt = decodedToken.Exp,
                        message = "Token valide"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "Token invalide ou malformé",
                        message = "Impossible de décoder le token"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation du token");
                return StatusCode(500, new { error = "Erreur lors de la validation du token" });
            }
        }
    }
}