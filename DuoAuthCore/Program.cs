// SPDX-FileCopyrightText: 2022 Cisco Systems, Inc. and/or its affiliates
//
// SPDX-License-Identifier: BSD-3-Clause

// Ce fichier définit le point d'entrée principal de l'application web ASP.NET Core pour l'exemple Duo Universal Prompt.

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

// Le namespace regroupe les classes liées à l'exemple DuoUniversal.
namespace DuoAuthCore
{
    // La classe Program contient la méthode Main, point d'entrée de l'application.
    public class Program
    {
        // La méthode Main est appelée automatiquement au démarrage de l'application.
        // Elle reçoit un tableau de chaînes de caractères (args) qui contient les arguments de la ligne de commande.
        public static void Main(string[] args)
        {
            // On crée un "host" (hôte) pour l'application web, puis on le construit et on l'exécute.
            // Cela démarre le serveur web et l'application ASP.NET Core.
            CreateHostBuilder(args).Build().Run();
        }

        // Cette méthode configure et retourne un IHostBuilder, qui prépare l'environnement d'exécution de l'application.
        // Host.CreateDefaultBuilder configure les paramètres par défaut (configuration, journalisation, etc.).
        // ConfigureWebHostDefaults configure les paramètres spécifiques à l'hébergement web (Kestrel, IIS, etc.).
        // webBuilder.UseStartup<Startup>() indique que la classe Startup sera utilisée pour configurer les services et le pipeline HTTP.
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

// En résumé :
// - Ce fichier lance l'application web ASP.NET Core.
// - Il configure l'environnement d'exécution et indique que la configuration principale se trouve dans la classe Startup.
// - C'est le point d'entrée standard pour une application ASP.NET Core moderne.
