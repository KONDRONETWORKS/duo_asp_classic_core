using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace DuoAuthCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DuoAuthController : ControllerBase
    {
        private readonly ILogger<DuoAuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DuoAuthController(
            ILogger<DuoAuthController> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Point d'entrée pour l'authentification Duo
        /// </summary>
        [HttpGet("duo-auth")]
        public async Task<IActionResult> DuoAuth(string username, string returnUrl)
        {
            try
            {
                _logger.LogInformation("Début authentification Duo pour {Username}", username);

                // 1. Initialiser la session
                var sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("username", username);
                HttpContext.Session.SetString("returnUrl", returnUrl);
                HttpContext.Session.SetString("session_id", sessionId);

                // 2. Générer l'état CSRF sécurisé
                var state = GenerateSecureState();
                HttpContext.Session.SetString("duo_state", state);

                // 3. Construire l'URL Duo
                var duoAuthUrl = GenerateDuoAuthUrl(username, state);

                _logger.LogInformation("Session initialisée - ID: {SessionId}, State: {State}", sessionId, state);
                _logger.LogInformation("Redirection vers Duo: {DuoUrl}", duoAuthUrl);

                // 4. Rediriger vers Duo
                return Redirect(duoAuthUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'initialisation Duo pour {Username}", username);
                return Redirect($"{returnUrl}?error=duo_init_failed&details={ex.Message}");
            }
        }

        /// <summary>
        /// Callback après authentification Duo
        /// </summary>
        [HttpGet("api/duoauth/callback")]
        public async Task<IActionResult> DuoCallback(string code, string state)
        {
            try
            {
                _logger.LogInformation("Callback Duo reçu - Code: {Code}, State: {State}", code, state);

                // 1. Récupérer les données de session
                var username = HttpContext.Session.GetString("username");
                var returnUrl = HttpContext.Session.GetString("returnUrl");
                var storedState = HttpContext.Session.GetString("duo_state");
                var sessionId = HttpContext.Session.GetString("session_id");

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(returnUrl))
                {
                    _logger.LogWarning("Session invalide - Username: {Username}, ReturnUrl: {ReturnUrl}", username, returnUrl);
                    return Redirect($"{returnUrl}?error=session_invalid");
                }

                // 2. Valider l'état CSRF
                if (string.IsNullOrEmpty(storedState) || storedState != state)
                {
                    _logger.LogWarning("État CSRF invalide - Stored: {StoredState}, Received: {ReceivedState}", storedState, state);
                    return Redirect($"{returnUrl}?error=csrf_invalid");
                }

                // 3. Simuler l'échange avec Duo (en mode dev)
                var duoResult = await SimulateDuoExchange(code, username);

                if (duoResult.Success)
                {
                    // 4. Générer le JWT token
                    var jwtToken = GenerateJwtToken(username, "duo_authenticated");

                    // 5. Nettoyer la session
                    HttpContext.Session.Clear();

                    // 6. Rediriger vers ASP Classic avec le token
                    var finalUrl = $"{returnUrl}?token={jwtToken}&status=success&username={username}";
                    _logger.LogInformation("Authentification Duo réussie pour {Username} - Redirection: {FinalUrl}", username, finalUrl);

                    return Redirect(finalUrl);
                }
                else
                {
                    _logger.LogWarning("Échec authentification Duo pour {Username}: {Result}", username, duoResult.Message);
                    return Redirect($"{returnUrl}?error=duo_auth_failed&details={duoResult.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du callback Duo");
                return Redirect($"{returnUrl}?error=callback_error&details={ex.Message}");
            }
        }

        /// <summary>
        /// Validation d'un token JWT
        /// </summary>
        [HttpGet("api/validate")]
        public IActionResult ValidateToken(string token)
        {
            try
            {
                _logger.LogInformation("Validation du token: {Token}", token);

                // En mode développement, validation simplifiée
                if (_configuration["Environment"] == "Development")
                {
                    if (!string.IsNullOrEmpty(token) && token.Length > 10)
                    {
                        _logger.LogInformation("Token validé en mode développement");
                        return Ok(new { valid = true, environment = "Development" });
                    }
                }

                // En mode production, validation stricte
                if (_configuration["Environment"] == "Production")
                {
                    // TODO: Implémenter la validation JWT stricte
                    _logger.LogWarning("Validation JWT stricte non implémentée");
                    return BadRequest(new { valid = false, error = "JWT validation not implemented" });
                }

                return BadRequest(new { valid = false, error = "Invalid token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation du token");
                return StatusCode(500, new { valid = false, error = "Internal server error" });
            }
        }

        /// <summary>
        /// Endpoint de santé
        /// </summary>
        [HttpGet("api/health")]
        public IActionResult Health()
        {
            var environment = _configuration["Environment"] ?? "Unknown";
            var timestamp = DateTime.UtcNow;
            
            _logger.LogInformation("Health check - Environment: {Environment}, Timestamp: {Timestamp}", environment, timestamp);
            
            return Ok(new
            {
                status = "OK",
                environment = environment,
                timestamp = timestamp,
                version = "1.0.0"
            });
        }

        #region Méthodes privées

        /// <summary>
        /// Génère un état CSRF sécurisé
        /// </summary>
        private string GenerateSecureState()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Génère l'URL d'authentification Duo
        /// </summary>
        private string GenerateDuoAuthUrl(string username, string state)
        {
            var clientId = _configuration["DuoSecurity:ClientId"];
            var apiHost = _configuration["DuoSecurity:ApiHost"];
            var redirectUri = _configuration["DuoSecurity:RedirectUri"];

            var baseUrl = $"https://{apiHost}/oauth/v1/authorize";
            var queryParams = new List<string>
            {
                $"client_id={Uri.EscapeDataString(clientId)}",
                $"redirect_uri={Uri.EscapeDataString(redirectUri)}",
                $"response_type=code",
                $"scope=openid",
                $"state={Uri.EscapeDataString(state)}"
            };

            if (!string.IsNullOrEmpty(username))
            {
                queryParams.Add($"login_hint={Uri.EscapeDataString(username)}");
            }

            return $"{baseUrl}?{string.Join("&", queryParams)}";
        }

        /// <summary>
        /// Simule l'échange avec Duo (mode développement)
        /// </summary>
        private async Task<DuoResult> SimulateDuoExchange(string code, string username)
        {
            // Simulation d'un délai réseau
            await Task.Delay(100);

            // En mode développement, accepter tous les codes
            if (_configuration["Environment"] == "Development")
            {
                _logger.LogInformation("Mode développement - Simulation d'authentification Duo réussie pour {Username}", username);
                return new DuoResult { Success = true, Message = "Development mode simulation" };
            }

            // En mode production, validation réelle
            // TODO: Implémenter l'intégration réelle avec Duo
            _logger.LogWarning("Mode production - Intégration Duo réelle non implémentée");
            return new DuoResult { Success = false, Message = "Production mode not implemented" };
        }

        /// <summary>
        /// Génère un token JWT simple
        /// </summary>
        private string GenerateJwtToken(string username, string role)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var payload = new
            {
                sub = username,
                role = role,
                iat = timestamp,
                exp = timestamp + 3600 // 1 heure
            };

            // En mode développement, token simple
            if (_configuration["Environment"] == "Development")
            {
                return $"dev_token_{username}_{timestamp}";
            }

            // En mode production, JWT réel
            // TODO: Implémenter la génération JWT réelle
            return $"prod_token_{username}_{timestamp}";
        }

        #endregion
    }

    /// <summary>
    /// Résultat de l'authentification Duo
    /// </summary>
    public class DuoResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
