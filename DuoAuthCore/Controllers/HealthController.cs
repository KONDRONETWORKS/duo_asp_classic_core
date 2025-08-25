using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DuoAuthCore.Providers;

namespace DuoAuthCore.Controllers
{
    /// <summary>
    /// Contrôleur pour les vérifications de santé de l'application
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IDuoClientProvider _duoClientProvider;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IDuoClientProvider duoClientProvider, ILogger<HealthController> logger)
        {
            _duoClientProvider = duoClientProvider;
            _logger = logger;
        }

        /// <summary>
        /// Vérification de santé basique
        /// </summary>
        /// <returns>Statut de santé de l'application</returns>
        [HttpGet]
        public IActionResult HealthCheck()
        {
            return Ok(new 
            { 
                status = "healthy", 
                message = "ASP.NET Core Duo Auth is running",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        /// <summary>
        /// Vérification de santé détaillée incluant la configuration Duo
        /// </summary>
        /// <returns>Statut détaillé de santé</returns>
        [HttpGet("detailed")]
        public IActionResult DetailedHealthCheck()
        {
            var healthStatus = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                services = new
                {
                    duo = CheckDuoConfiguration(),
                    session = "active",
                    database = "n/a"
                }
            };

            return Ok(healthStatus);
        }

        /// <summary>
        /// Vérification de la configuration Duo
        /// </summary>
        /// <returns>Statut de la configuration Duo</returns>
        [HttpGet("duo-config")]
        public IActionResult DuoConfigCheck()
        {
            try
            {
                var client = _duoClientProvider.GetDuoClient();
                var config = new
                {
                    status = "configured",
                    clientId = _duoClientProvider.GetClientId(),
                    apiHost = _duoClientProvider.GetApiHost(),
                    redirectUri = _duoClientProvider.GetRedirectUri(),
                    message = "Configuration Duo valide"
                };

                _logger.LogInformation("Configuration Duo vérifiée avec succès");
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de la configuration Duo");
                return StatusCode(500, new
                {
                    status = "error",
                    message = "Erreur de configuration Duo",
                    details = ex.Message
                });
            }
        }

        private object CheckDuoConfiguration()
        {
            try
            {
                var client = _duoClientProvider.GetDuoClient();
                return new { status = "configured", message = "Configuration Duo valide" };
            }
            catch (Exception ex)
            {
                return new { status = "error", message = ex.Message };
            }
        }
    }
}
