// SPDX-FileCopyrightText: 2022 Cisco Systems, Inc. and/or its affiliates
//
// SPDX-License-Identifier: BSD-3-Clause

using System; // Pour Guid, Exception
using System.Collections.Generic; // Pour Dictionary
using Microsoft.AspNetCore.Http; // Pour HttpContextAccessor
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DuoUniversal;
using System.IO; // AJOUTEZ cette ligne
using Microsoft.AspNetCore.DataProtection; // AJOUTEZ cette ligne
using DuoAuthCore.Services; // AJOUTEZ cette ligne

namespace DuoAuthCore
{
    // La classe Startup configure l'application ASP.NET Core.
    public class Startup
    {
        // Constructeur : il reçoit la configuration de l'application (appsettings.json, variables d'environnement, etc.)
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // Propriété qui permet d'accéder à la configuration dans toute la classe.
        public IConfiguration Configuration { get; }

        // ConfigureServices : méthode appelée au démarrage pour enregistrer les services nécessaires à l'application.
        // Elle permet d'ajouter des services à l'injection de dépendances.
        public void ConfigureServices(IServiceCollection services)
        {
            // Création d'un fournisseur de client Duo à partir de la configuration.
            // Ce fournisseur sera injecté partout où l'interface IDuoClientProvider est demandée.
            var duoClientProvider = new DuoClientProvider(Configuration);
            services.AddSingleton<IDuoClientProvider>(duoClientProvider);
            // OR // Configuration Duo Client Provider
            // services.AddSingleton<IDuoClientProvider, DuoClientProvider>();

            // DEBUG: Affichez la configuration Duo
            var duoSection = Configuration.GetSection("Duo");
            Console.WriteLine($"ClientId: {duoSection["ClientId"]}");
            Console.WriteLine($"ApiHost: {duoSection["ApiHost"]}");

            // Configuration DataProtection
            services.AddDataProtection()
                .PersistKeysToFileSystem(new System.IO.DirectoryInfo(@"C:\temp-keys\"))
                .SetApplicationName("DuoAuthCore");

            services.AddSingleton<TempAuthStorage>();

            // Ajout d'un cache mémoire distribué pour la gestion de la session.
            services.AddDistributedMemoryCache();

            // Configuration de la gestion de session :
            // - Durée d'inactivité de 60 minutes
            // - Cookie HTTP only (non accessible en JavaScript)
            // - Cookie essentiel (toujours envoyé)
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                // options.Cookie.Secure = false; // Cette propriété n'existe pas dans CookieBuilder pour .NET Core 3.1/5/6
                // Pour définir le mode sécurisé du cookie, il faut utiliser la propriété SecurePolicy :
                options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Mettez à Always en production avec HTTPS
                options.Cookie.Name = "DuoAuth.Session"; // Nom explicite

            });
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            // Ajout du support des contrôleurs API
            services.AddControllers();

            // Ajout de l'accesseur de contexte HTTP pour les contrôleurs
            services.AddHttpContextAccessor();
        }

        // Configure : méthode qui définit le pipeline HTTP de l'application.
        // Elle est appelée au démarrage et permet de configurer l'ordre des middlewares.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDuoClientProvider duoProvider)
        {
            // Si l'environnement est "Développement", on affiche la page d'erreur détaillée.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Testez la configuration au démarrage
            try
            {
                var client = duoProvider.GetDuoClient();
                Console.WriteLine("✅ Configuration Duo VALIDE");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Configuration Duo ERREUR: {ex.Message}");
            }

            // Redirige automatiquement les requêtes HTTP vers HTTPS.
            // app.UseHttpsRedirection();

            // Active le routage (permet de diriger les requêtes vers les bons contrôleurs/pages)
            app.UseRouting();

            // Dans Configure() Activez CORS avant la session si nécessaire
            app.UseCors("AllowAll");

            // Il faut activer la session AVANT d'accéder à context.Session !
            app.UseSession();

            // Middleware de debug de la session, APRES app.UseSession()
           // Middleware de debug de la session - MAINTENANT APRÈS UseSession()
            app.Use(async (context, next) =>
            {
                try
                {
                    var session = context.Session;
                    if (session != null && session.IsAvailable)
                    {
                        Console.WriteLine($"Session ID: {session.Id}");
                        Console.WriteLine($"Session Keys: {string.Join(", ", session.Keys)}");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ La session n'est pas disponible pour cette requête.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"⚠️ Erreur session: {ex.Message}");
                }
                await next();
            });


            // Configure les endpoints (points d'entrée) pour utiliser les Razor Pages et les contrôleurs API.
            app.UseEndpoints(endpoints =>
            {
                // endpoints.MapRazorPages();
                endpoints.MapControllers();
            });

        }
    }

    // Interface qui définit un fournisseur de client Duo.
    // Elle impose la présence d'une méthode GetDuoClient qui retourne un objet Client.
    public interface IDuoClientProvider
    {
        Client GetDuoClient();
        string GetClientId();
        string GetApiHost();
        string GetRedirectUri();
    }

    // Classe interne qui implémente l'interface IDuoClientProvider.
    // Elle lit la configuration Duo et construit un client Duo prêt à l'emploi.
    internal class DuoClientProvider : IDuoClientProvider
    {
        // Propriétés privées pour stocker les paramètres nécessaires à la connexion Duo.
        private string ClientId { get; }
        private string ClientSecret { get; }
        private string ApiHost { get; }
        private string RedirectUri { get; }

        // Constructeur : lit les paramètres Duo dans la configuration (section "Duo").
        public DuoClientProvider(IConfiguration config)
        {
            var duoSection = config.GetSection("Duo");
            ClientId = duoSection.GetValue<string>("ClientId");
            ClientSecret = duoSection.GetValue<string>("ClientSecret");
            ApiHost = duoSection.GetValue<string>("ApiHost");
            RedirectUri = duoSection.GetValue<string>("RedirectUri");
        }

        // Méthode qui retourne un objet Client configuré pour communiquer avec Duo.
        // Elle vérifie que tous les paramètres sont présents, sinon elle lève une exception explicite.
        public Client GetDuoClient()
        {
            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new DuoException("A 'ClientId' configuration value is required in the appsettings file.");
            }
            if (string.IsNullOrWhiteSpace(ClientSecret))
            {
                throw new DuoException("A 'ClientSecret' configuration value is required in the appsettings file.");
            }
            if (string.IsNullOrWhiteSpace(ApiHost))
            {
                throw new DuoException("An 'ApiHost' configuration value is required in the appsettings file.");
            }
            if (string.IsNullOrWhiteSpace(RedirectUri))
            {
                throw new DuoException("A 'RedirectUri' configuration value is required in the appsettings file.");
            }

            // Création et retour d'un client Duo configuré.
            return new ClientBuilder(ClientId, ClientSecret, ApiHost, RedirectUri).Build();
        }
        public string GetClientId() => ClientId;
        public string GetApiHost() => ApiHost;
        public string GetRedirectUri() => RedirectUri;    
    }
}
