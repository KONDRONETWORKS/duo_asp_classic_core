using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using DuoUniversal;
using System;

namespace DuoAuthCore.Services
{
    public class DuoClientProvider : IDuoClientProvider
    {
        private string ClientId { get; }
        private string ClientSecret { get; }
        private string ApiHost { get; }
        private string RedirectUri { get; }

        public DuoClientProvider(IConfiguration config)
        {
            var duoSection = config.GetSection("Duo");

            // Les clés dans appsettings.json sont "ClientId", "ClientSecret", "ApiHost", "RedirectUri"
            ClientId = duoSection.GetValue<string>("ClientId");
            ClientSecret = duoSection.GetValue<string>("ClientSecret");
            ApiHost = duoSection.GetValue<string>("ApiHost");
            RedirectUri = duoSection.GetValue<string>("RedirectUri");

            // DEBUG : Affichez les valeurs configurées
            Console.WriteLine("=== CONFIGURATION DUO ===");
            Console.WriteLine($"ClientId : '{ClientId}' (Longueur : {ClientId?.Length})");
            Console.WriteLine($"ClientSecret : '{ClientSecret?.Substring(0, Math.Min(10, ClientSecret?.Length ?? 0))}...' (Longueur : {ClientSecret?.Length})");
            Console.WriteLine($"ApiHost : '{ApiHost}'");
            Console.WriteLine($"RedirectUri : '{RedirectUri}'");
            Console.WriteLine("=========================");

            // Validation basique
            if (string.IsNullOrEmpty(ClientId) || ClientId.Length != 20)
            {
                throw new Exception($"ClientId invalide. Doit avoir 20 caractères. Actuel : '{ClientId}' (Longueur : {ClientId?.Length})");
            }
        }

        public Client GetDuoClient()
        {
            try
            {
                Console.WriteLine("Création du client Duo...");

                var client = new ClientBuilder(ClientId, ClientSecret, ApiHost, RedirectUri)
                    .Build();

                Console.WriteLine("Client Duo créé avec succès !");
                return client;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR lors de la création du client Duo : {ex.Message}");
                throw;
            }
        }
    }

    // Service simple de stockage temporaire pour l'authentification
    public class TempAuthStorage
    {
        private static readonly ConcurrentDictionary<string, (string State, string Username, DateTime Expiry)>
            _storage = new ConcurrentDictionary<string, (string, string, DateTime)>();

        public void Store(string state, string username)
        {
            _storage[state] = (state, username, DateTime.UtcNow.AddMinutes(5));
        }

        public string GetUsername(string state)
        {
            if (_storage.TryGetValue(state, out var data) && data.Expiry > DateTime.UtcNow)
            {
                return data.Username;
            }
            return null;
        }
    }

    public interface IDuoClientProvider
    {
        Client GetDuoClient();
    }
}