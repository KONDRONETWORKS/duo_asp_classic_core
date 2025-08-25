//OPtion 01 dans Provider/
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

// Option 02 dans Services/
// using Microsoft.Extensions.Configuration;
// using System.Collections.Concurrent;
// using DuoUniversal;
// using System;

// namespace DuoAuthCore.Services
// {
//     public class DuoClientProvider : IDuoClientProvider
//     {
//         private string ClientId { get; }
//         private string ClientSecret { get; }
//         private string ApiHost { get; }
//         private string RedirectUri { get; }

//         public DuoClientProvider(IConfiguration config)
//         {
//             var duoSection = config.GetSection("Duo");

//             // Les clés dans appsettings.json sont "ClientId", "ClientSecret", "ApiHost", "RedirectUri"
//             ClientId = duoSection.GetValue<string>("ClientId");
//             ClientSecret = duoSection.GetValue<string>("ClientSecret");
//             ApiHost = duoSection.GetValue<string>("ApiHost");
//             RedirectUri = duoSection.GetValue<string>("RedirectUri");

//             // DEBUG : Affichez les valeurs configurées
//             Console.WriteLine("=== CONFIGURATION DUO ===");
//             Console.WriteLine($"ClientId : '{ClientId}' (Longueur : {ClientId?.Length})");
//             Console.WriteLine($"ClientSecret : '{ClientSecret?.Substring(0, Math.Min(10, ClientSecret?.Length ?? 0))}...' (Longueur : {ClientSecret?.Length})");
//             Console.WriteLine($"ApiHost : '{ApiHost}'");
//             Console.WriteLine($"RedirectUri : '{RedirectUri}'");
//             Console.WriteLine("=========================");

//             // Validation basique
//             if (string.IsNullOrEmpty(ClientId) || ClientId.Length != 20)
//             {
//                 throw new Exception($"ClientId invalide. Doit avoir 20 caractères. Actuel : '{ClientId}' (Longueur : {ClientId?.Length})");
//             }
//         }

//         public Client GetDuoClient()
//         {
//             try
//             {
//                 Console.WriteLine("Création du client Duo...");

//                 var client = new ClientBuilder(ClientId, ClientSecret, ApiHost, RedirectUri)
//                     .Build();

//                 Console.WriteLine("Client Duo créé avec succès !");
//                 return client;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"ERREUR lors de la création du client Duo : {ex.Message}");
//                 throw;
//             }
//         }
//     }

//     // Service simple de stockage temporaire pour l'authentification
//     public class TempAuthStorage
//     {
//         private static readonly ConcurrentDictionary<string, (string State, string Username, DateTime Expiry)>
//             _storage = new ConcurrentDictionary<string, (string, string, DateTime)>();

//         public void Store(string state, string username)
//         {
//             _storage[state] = (state, username, DateTime.UtcNow.AddMinutes(5));
//         }

//         public string GetUsername(string state)
//         {
//             if (_storage.TryGetValue(state, out var data) && data.Expiry > DateTime.UtcNow)
//             {
//                 return data.Username;
//             }
//             return null;
//         }
//     }

//     public interface IDuoClientProvider
//     {
//         Client GetDuoClient();
//     }
// }
