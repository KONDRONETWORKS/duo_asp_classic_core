using System;
using System.Collections.Concurrent;
using System.Linq;

namespace DuoAuthCore.Services
{
    /// <summary>
    /// Service de stockage temporaire pour l'authentification
    /// Utilise un dictionnaire thread-safe pour stocker les données d'auth
    /// </summary>
    public class TempAuthStorage
    {
        private readonly ConcurrentDictionary<string, AuthData> _storage = new();

        /// <summary>
        /// Stocke les données d'authentification avec une clé unique
        /// </summary>
        /// <param name="key">Clé unique pour récupérer les données</param>
        /// <param name="data">Données d'authentification à stocker</param>
        /// <param name="expirationMinutes">Durée de vie en minutes (défaut: 30)</param>
        public void Store(string key, AuthData data, int expirationMinutes = 30)
        {
            data.ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
            _storage.AddOrUpdate(key, data, (k, v) => data);
        }

        /// <summary>
        /// Récupère les données d'authentification par clé
        /// </summary>
        /// <param name="key">Clé pour récupérer les données</param>
        /// <returns>Données d'authentification ou null si expirées/inexistantes</returns>
        public AuthData? Retrieve(string key)
        {
            if (_storage.TryGetValue(key, out var data))
            {
                if (data.ExpiresAt > DateTime.UtcNow)
                {
                    return data;
                }
                else
                {
                    // Nettoyer les données expirées
                    _storage.TryRemove(key, out _);
                }
            }
            return null;
        }

        /// <summary>
        /// Supprime les données d'authentification par clé
        /// </summary>
        /// <param name="key">Clé des données à supprimer</param>
        public void Remove(string key)
        {
            _storage.TryRemove(key, out _);
        }

        /// <summary>
        /// Nettoie toutes les données expirées
        /// </summary>
        public void CleanupExpired()
        {
            var expiredKeys = _storage
                .Where(kvp => kvp.Value.ExpiresAt <= DateTime.UtcNow)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _storage.TryRemove(key, out _);
            }
        }
    }

    /// <summary>
    /// Données d'authentification stockées temporairement
    /// </summary>
    public class AuthData
    {
        public string Username { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
    }
}
