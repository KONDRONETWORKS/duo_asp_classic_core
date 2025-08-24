using System; // Pour Console, Guid, etc.
using System.Threading.Tasks; // Pour async/await
using Microsoft.AspNetCore.Http; // Pour HttpContextAccessor
using Microsoft.AspNetCore.Mvc; // Pour ApiController, ControllerBase
using Microsoft.Extensions.Configuration; // Pour IConfiguration
using DuoUniversal; // Pour Client, IdToken, etc.

namespace DuoAuthCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DuoAuthController : ControllerBase
    {
        private readonly IDuoClientProvider _duoClientProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public DuoAuthController(IDuoClientProvider duoClientProvider, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _duoClientProvider = duoClientProvider;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", message = "ASP.NET Core Duo Auth is running" });
        }

        [HttpGet("validate")]
        public IActionResult ValidateToken([FromQuery] string token)
        {
            // Ici, vous validerez le token JWT
            return Ok(new { valid = true, message = "Token is valid" });
        }

        /// <summary>
        /// Endpoint direct pour l'authentification Duo depuis ASP Classic
        /// Redirige directement vers Duo après avoir initialisé la session
        /// </summary>
        [HttpGet("duo-auth")]
        public IActionResult DuoAuth([FromQuery] string username, [FromQuery] string returnUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return BadRequest(new { Error = "Nom d'utilisateur requis" });
                }

                var session = _httpContextAccessor.HttpContext.Session;
                
                // Stocker le nom d'utilisateur en session
                session.SetString("_Username", username);
                session.SetString("_LegacyApp", "true");

                // Générer un état unique pour cette session
                var state = Guid.NewGuid().ToString();
                session.SetString("_State", state);

                // Utiliser la méthode CORRECTE GenerateAuthUri (comme dans LoginModel)
                var duoClient = _duoClientProvider.GetDuoClient();
                var authUrl = duoClient.GenerateAuthUri(username, state);
                
                Console.WriteLine($"Redirection directe vers Duo pour l'utilisateur: {username}");
                Console.WriteLine($"URL Duo: {authUrl}");

                // Rediriger directement vers Duo
                return Redirect(authUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur dans duo-auth: {ex.Message}");
                return StatusCode(500, new { Error = "Erreur lors de l'initialisation de l'authentification Duo", Details = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint de callback après authentification Duo
        /// Redirige vers l'application ASP Classic avec le token et username
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                return BadRequest(new { Error = "Code ou state manquant" });

            var sessionUsername = HttpContext.Session.GetString("_Username");
            if (string.IsNullOrEmpty(sessionUsername))
                return BadRequest(new { Error = "Session invalide, username introuvable" });

            try
            {
                var duoClient = _duoClientProvider.GetDuoClient();
                var idTokenObj = await duoClient.ExchangeAuthorizationCodeFor2faResult(code, sessionUsername);

                if (idTokenObj == null)
                    return BadRequest(new { Error = "Échec de l'échange du code d'autorisation" });

                // Récupérer l'AuthResult depuis IdToken
                var authResultProp = idTokenObj.GetType().GetProperty("AuthResult");
                if (authResultProp == null)
                    return BadRequest(new { Error = "Propriété 'AuthResult' introuvable" });

                var authResult = authResultProp.GetValue(idTokenObj);

                // Vérifier le résultat MFA
                var result = authResult.GetType().GetProperty("Result")?.GetValue(authResult) as string;
                var statusMsg = authResult.GetType().GetProperty("StatusMsg")?.GetValue(authResult) as string;

                if (result != "allow")
                {
                    return Unauthorized(new { Error = "Authentification Duo refusée", Status = statusMsg ?? "Unknown" });
                }

                // Extraire le token JWT depuis IdToken.IdToken
                string idToken = null;
                var idTokenProperty = idTokenObj.GetType().GetProperty("IdToken");
                if (idTokenProperty != null)
                    idToken = idTokenProperty.GetValue(idTokenObj) as string;

                if (string.IsNullOrEmpty(idToken) || idToken == "DuoUniversal.IdToken")
                {
                    // Fallback : recherche récursive du JWT dans l'objet
                    idToken = ExtractTokenFromIdToken(idTokenObj);
                }

                if (string.IsNullOrEmpty(idToken))
                    return BadRequest(new { Error = "Token JWT manquant dans AuthResult" });

                Console.WriteLine($"JWT reçu: {idToken.Substring(0, Math.Min(50, idToken.Length))}...");

                var redirectUrl = $"http://localhost/asp/duo_callback.asp?token={Uri.EscapeDataString(idToken)}&username={Uri.EscapeDataString(sessionUsername)}";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Erreur durant le callback Duo", Details = ex.Message });
            }
        }






        // Méthode helper pour extraire le token de l'objet IdToken
        // private string ExtractTokenFromIdToken(object idTokenObj)
        // {
        //     Console.WriteLine("=== DEBUG EXTRACT TOKEN ===");
            
        //     // Essayer d'abord ToString()
        //     string token = idTokenObj.ToString();
        //     Console.WriteLine($"ToString() result: {token}");
            
        //     try
        //     {
        //         // Accéder à la propriété AuthResult
        //         var authResultProperty = idTokenObj.GetType().GetProperty("AuthResult");
        //         if (authResultProperty != null)
        //         {
        //             var authResult = authResultProperty.GetValue(idTokenObj);
        //             Console.WriteLine($"AuthResult type: {authResult?.GetType().FullName}");
                    
        //             if (authResult != null)
        //             {
        //                 // Chercher une propriété contenant le token dans AuthResult
        //                 var authResultProperties = authResult.GetType().GetProperties();
        //                 foreach (var prop in authResultProperties)
        //                 {
        //                     var value = prop.GetValue(authResult);
        //                     Console.WriteLine($"- AuthResult.{prop.Name}: {value} (Type: {value?.GetType().Name})");
                            
        //                     if (value is string stringValue && stringValue.Length > 100 && stringValue.Contains('.'))
        //                     {
        //                         if (stringValue.Split('.').Length == 3) // Vérifier le format JWT
        //                         {
        //                             token = stringValue;
        //                             Console.WriteLine($"JWT trouvé dans AuthResult.{prop.Name}");
        //                             break;
        //                         }
        //                     }
        //                 }
        //             }
        //         }
                
        //         // Si toujours pas trouvé, chercher dans les autres propriétés
        //         if (token == "DuoUniversal.IdToken")
        //         {
        //             var properties = idTokenObj.GetType().GetProperties();
        //             foreach (var prop in properties)
        //             {
        //                 var value = prop.GetValue(idTokenObj);
        //                 Console.WriteLine($"- {prop.Name}: {value} (Type: {value?.GetType().Name})");
                        
        //                 if (value is string stringValue && stringValue.Length > 100 && stringValue.Contains('.'))
        //                 {
        //                     if (stringValue.Split('.').Length == 3)
        //                     {
        //                         token = stringValue;
        //                         Console.WriteLine($"JWT trouvé dans {prop.Name}");
        //                         break;
        //                     }
        //                 }
        //             }
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Erreur lors de l'extraction: {ex.Message}");
        //     }
            
        //     Console.WriteLine($"Final token: {(string.IsNullOrEmpty(token) ? "NULL" : token.Substring(0, Math.Min(50, token.Length)) + "...")}");
        //     Console.WriteLine("========================");
            
        //     return token;
        // }
        // private string ExtractTokenFromIdToken(object idTokenObj)
        // {
        //     try
        //     {
        //         // Méthode 1: Accéder à AuthResult puis à ses propriétés
        //         var authResultProp = idTokenObj.GetType().GetProperty("AuthResult");
        //         if (authResultProp != null)
        //         {
        //             var authResult = authResultProp.GetValue(idTokenObj);
                    
        //             // Essayer les propriétés courantes dans AuthResult
        //             var idTokenProp = authResult.GetType().GetProperty("IdToken");
        //             var accessTokenProp = authResult.GetType().GetProperty("AccessToken");
        //             var jwtProp = authResult.GetType().GetProperty("Jwt");
                    
        //             if (idTokenProp != null)
        //             {
        //                 var value = idTokenProp.GetValue(authResult) as string;
        //                 if (!string.IsNullOrEmpty(value)) return value;
        //             }
                    
        //             if (accessTokenProp != null)
        //             {
        //                 var value = accessTokenProp.GetValue(authResult) as string;
        //                 if (!string.IsNullOrEmpty(value)) return value;
        //             }
                    
        //             if (jwtProp != null)
        //             {
        //                 var value = jwtProp.GetValue(authResult) as string;
        //                 if (!string.IsNullOrEmpty(value)) return value;
        //             }
        //         }
                
        //         // Méthode 2: Fallback vers ToString()
        //         return idTokenObj.ToString();
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Erreur extraction token: {ex.Message}");
        //         return idTokenObj.ToString();
        //     }
        // }
        private string ExtractTokenFromIdToken(object idTokenObj)
        {
            return FindJwtTokenRecursive(idTokenObj, 0) ?? idTokenObj.ToString();
        }

        private string FindJwtTokenRecursive(object obj, int depth)
        {
            if (depth > 3) return null; // Limite de récursion
            
            if (obj == null) return null;
            
            // Si c'est une string qui ressemble à un JWT
            if (obj is string str && str.Length > 100 && str.Split('.').Length == 3)
            {
                return str;
            }
            
            // Explorer les propriétés de l'objet
            try
            {
                var properties = obj.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(obj);
                    var result = FindJwtTokenRecursive(value, depth + 1);
                    if (result != null) return result;
                }
            }
            catch
            {
                // Ignorer les erreurs de réflexion
            }
            
            return null;
        }
    }
}