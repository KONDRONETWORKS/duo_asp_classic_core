# Duo Universal Prompt C# Sample Application

This example application demonstrates how to integrate the Duo Universal C# client into a simple ASP.NET web application.

### Caveats

The Duo Universal C# client provides asynchronous methods and that is the paradigm demonstrated in the example.  If you need to use the C# client from a synchronous web application, you will need to wrap the async calls in synchronizing code.

A detailed investigation into possible approaches can be found on [MSDN](https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development#transform-synchronous-to-asynchronous-code).

Users of this repository have reported that the following approach works in their ASP.NET web app:

`var token = Task.Run(async () => { return await duoClient.ExchangeAuthorizationCodeFor2faResult(context.Request["code"], username); }).Result;`

Duo has used the following approach in internal products:

`var _idToken = duoClient.ExchangeAuthorizationCodeFor2faResult(duoCode, username).GetAwaiter().GetResult();`

## Build

### With the .NET CLI
From the DuoAuthCore directory run:

`dotnet build`

## Run

In order to run this project, ensure the values in `DuoAuthCore/appsettings.json` (or `appsettings.Development.json` if you prefer) 
are filled out with the values from the Duo Admin Panel (ClientId, ClientSecret, ApiHost).

### With the .NET CLI
From the DuoAuthCore base directory run the following to start the server:

`dotnet run --framework net6.0`

Or you can use `--framework netcoreapp3.1` if you prefer.

## Interact

Navigate to <https://localhost:5001> or <http://localhost:5000> to see a mock user login form.  Enter a Duo username and any password to initiate Duo 2FA.

// SPDX-FileCopyrightText: 2022 Cisco Systems, Inc. and/or its affiliates
//
// SPDX-License-Identifier: BSD-3-Clause


# STARTUP.CS 
/*
Explication détaillée du code Startup.cs pour l'exemple DuoUniversal :

Ce fichier configure l'application web ASP.NET Core pour intégrer l'authentification Duo Universal Prompt.

1. Espaces de noms utilisés :
- System, Microsoft.AspNetCore.*, Microsoft.Extensions.* : Fournissent les classes nécessaires pour configurer et exécuter une application web ASP.NET Core.

2. Namespace DuoAuthCore :
- Contient toutes les classes liées à l'exemple.

3. Classe Startup :
- C'est la classe centrale de configuration de l'application ASP.NET Core.

a) Constructeur Startup(IConfiguration configuration)
- Reçoit la configuration de l'application (fichier appsettings.json, variables d'environnement, etc.).
- Stocke cette configuration dans la propriété Configuration.

b) Propriété Configuration
- Permet d'accéder à la configuration dans toute la classe.

c) Méthode ConfigureServices(IServiceCollection services)
- Configure les services utilisés par l'application (injection de dépendances).
- Crée une instance de DuoClientProvider (qui encapsule la logique de création du client Duo) à partir de la configuration, et l'enregistre comme singleton pour l'interface IDuoClientProvider.
- Ajoute un cache mémoire distribué (AddDistributedMemoryCache) pour stocker des données de session.
- Configure la gestion de session (AddSession) : durée d'inactivité de 60 minutes, cookie HTTP only et essentiel.
- Ajoute le support des Razor Pages (AddRazorPages), qui sont utilisées pour générer les pages web.

d) Méthode Configure(IApplicationBuilder app, IWebHostEnvironment env)
- Configure le pipeline HTTP de l'application.
- Si l'environnement est en développement, active la page d'exception développeur.
- Force la redirection HTTPS (UseHttpsRedirection).
- Sert les fichiers statiques (UseStaticFiles).
- Active le routage (UseRouting).
- Active la gestion de session (UseSession).
- Configure les endpoints pour utiliser les Razor Pages (endpoints.MapRazorPages()).

4. Interface IDuoClientProvider
- Définit une méthode GetDuoClient() qui retourne un objet Client (pour interagir avec Duo).

5. Classe interne DuoClientProvider
- Implémente IDuoClientProvider.
- Stocke les paramètres nécessaires à la connexion Duo : ClientId, ClientSecret, ApiHost, RedirectUri.
- Dans le constructeur, lit ces paramètres depuis la section "Duo" de la configuration.
- Méthode GetDuoClient() : 
    - Vérifie que chaque paramètre est bien présent, sinon lève une exception explicite.
    - Crée et retourne un objet Client via un ClientBuilder, configuré avec les paramètres lus.

Résumé :
Ce code configure une application ASP.NET Core pour utiliser l'authentification Duo Universal Prompt. Il lit les paramètres de connexion Duo depuis la configuration, prépare un fournisseur de client Duo injectable, configure la gestion de session et le routage, et prépare l'application à servir des pages Razor. Toute la logique d'accès à Duo est centralisée dans DuoClientProvider, ce qui facilite la maintenance et la réutilisation.
*/
