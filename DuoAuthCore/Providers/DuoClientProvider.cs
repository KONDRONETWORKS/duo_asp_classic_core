using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using DuoUniversal;

namespace DuoAuthCore.Providers
{
    /// <summary>
    /// Interface pour fournir une instance configurée de DuoUniversal.Client
    /// </summary>
    public interface IDuoClientProvider
    {
        DuoUniversal.Client GetDuoClient();
        string GetClientId();
        string GetApiHost();
        string GetRedirectUri();
    }

    /// <summary>
    /// Classe qui implémente l'interface IDuoClientProvider.
    /// Configure et fournit une instance de DuoUniversal.Client
    /// </summary>
    public class DuoClientProvider : IDuoClientProvider
    {
        private readonly IConfiguration _configuration;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _apiHost;
        private readonly string _redirectUri;

        public DuoClientProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            
            // Lecture de la configuration depuis appsettings.json
            _clientId = _configuration["Duo:ClientId"] ?? throw new InvalidOperationException("Duo:ClientId non configuré");
            _clientSecret = _configuration["Duo:ClientSecret"] ?? throw new InvalidOperationException("Duo:ClientSecret non configuré");
            _apiHost = _configuration["Duo:ApiHost"] ?? throw new InvalidOperationException("Duo:ApiHost non configuré");
            _redirectUri = _configuration["Duo:RedirectUri"] ?? throw new InvalidOperationException("Duo:RedirectUri non configuré");
        }

        /// <summary>
        /// Crée et retourne une instance configurée de DuoUniversal.Client
        /// </summary>
        /// <returns>Instance configurée de DuoUniversal.Client</returns>
        public DuoUniversal.Client GetDuoClient()
        {
            try
            {
                // Utiliser le ClientBuilder avec désactivation de la validation SSL stricte
                // pour éviter les problèmes de certificats embarqués
                var client = new DuoUniversal.ClientBuilder(_clientId, _clientSecret, _apiHost, _redirectUri)
                    .DisableSslCertificateValidation() // Désactiver la validation SSL stricte
                    .Build();

                return client;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors de la création du client Duo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retourne le Client ID configuré
        /// </summary>
        /// <returns>Client ID</returns>
        public string GetClientId()
        {
            return _clientId;
        }

        /// <summary>
        /// Retourne l'API Host configuré
        /// </summary>
        /// <returns>API Host</returns>
        public string GetApiHost()
        {
            return _apiHost;
        }

        /// <summary>
        /// Retourne l'URI de redirection configuré
        /// </summary>
        /// <returns>URI de redirection</returns>
        public string GetRedirectUri()
        {
            return _redirectUri;
        }
    }
}
