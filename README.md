# duo_asp_classic_core

Voici un README complet et détaillé que vous pouvez utiliser sur GitHub pour votre projet d’intégration Duo avec ASP Classic et ASP.NET Core :

***

# 🚀 Mode Opératoire Complet d’Intégration Duo

## 🎯 Vue d’ensemble du Flux d’Authentification

```plaintext
ASP Classic login.asp
    ↓
→ POST avec username/password
    ↓
→ Validation des credentials
    ↓
→ Redirection vers http://localhost:5000/api/duoauth/duo-auth?username=xxx&returnUrl=xxx
    ↓
→ Votre API Core (/duo-auth) reçoit la requête
    ↓
→ Initialise la session et génère l’état (state)
    ↓
→ Redirige directement vers Duo Universal Prompt
    ↓
→ L’utilisateur valide son MFA sur Duo
    ↓
→ Duo redirige vers /api/duoauth/callback (callback dans Core)
    ↓
→ Le backend Core traite le callback, valide MFA, extrait et transmet le JWT
    ↓
→ Redirection vers duo_callback.asp (ASP Classic) avec token et username
    ↓
→ ASP Classic continue son traitement avec le JWT reçu
```

***

## 🔧 Configuration Initiale

### 1. Prérequis Système

- IIS activé avec support ASP Classic
- .NET Core 6.0+ installé (SDK et Runtime)
- Compte développeur Duo Security actif avec accès API (ClientID, ClientSecret, ApiHost)
- Éditeur de code (Visual Studio Code recommandé)
- Ports 80 (IIS) et 5000 (API Core) libres et ouverts

### 2. Configuration des Secrets et Variables

#### ASP Classic : Fichier `duo_config.txt`

```
DUO_CLIENT_ID=VotreClientID
DUO_CLIENT_SECRET=VotreClientSecret
DUO_API_HOST=api-votre-instance.duosecurity.com
DUO_REDIRECT_URI=http://localhost:5000/api/duoauth/callback
```

#### API Core : Fichier `appsettings.json`

```json
{
  "DuoSecurity": {
    "ClientId": "VotreClientID",
    "ClientSecret": "VotreClientSecret",
    "ApiHost": "api-votre-instance.duosecurity.com",
    "RedirectUri": "http://localhost:5000/api/duoauth/callback"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 3. Configuration ASP.NET Core

- Activer le middleware de session avec :

```csharp
services.AddDistributedMemoryCache();
services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Use Always en prod https
    options.Cookie.Name = "DuoAuth.Session";
});
```

- Dans `Configure` :

```csharp
app.UseRouting();
app.UseCors("AllowAll");
app.UseSession();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
```

***

## 🚀 Démarrage de l’Application

### Exécution API Core

```powershell
cd C:\inetpub\wwwroot\asp\DuoAuthCore
dotnet restore
dotnet run --urls="http://localhost:5000"
```

✅ Test santé API :

```bash
curl http://localhost:5000/api/duoauth/health
```

### Test ASP Classic

- Page de login : `http://localhost/asp/login.asp`
- Page protégée : `http://localhost/asp/protected.asp`

***

## 🎯 Points clés d’intégration

- Le login initial ASP Classic valide l’utilisateur et redirige vers l’API Core (`/duo-auth`) avec username.
- L’API Core initialise la session, génère un état unique, puis redirige vers Duo Universal Prompt.
- Après validation MFA, Duo redirige vers API Core `/duoauth/callback`.
- Le callback valide le code et extrait le JWT, si MFA autorisé, il redirige vers ASP Classic avec token et username.
- La page `duo_callback.asp` valide la forme du token et démarre la session utilisateur ASP Classic.

***

## ⚠️ Erreurs courantes & solutions

- **Token = "DuoUniversal.IdToken" en callback :**  
  Mauvaise extraction du token JWT dans API Core, corriger la méthode `Callback` pour bien extraire `AuthResult` puis `IdToken` (vrai JWT).

- **Session ASP.NET Core non persistante :**  
  Confirmer middleware session bien inséré et ordre d’appel dans `Configure`. Vérifier que le cookie de session est envoyé et reçu.

- **Timeout ou erreur réseau avec Duo :**  
  Vérifier horaires serveur (NTP), ports ouverts, configuration correcte des clés API,

***

## 🛠 Structure des fichiers

```plaintext
C:\inetpub\wwwroot\asp
├── duo_config.txt          # Configuration secrets ASP Classic
├── duo_callback.asp        # Page callback ASP Classic, validation token
├── login.asp               # Page login initial
├── protected.asp           # Page protégée accessible après MFA
├── includes\               # Inclut fichiers ASP Classic (auth.asp, duo_auth.asp, etc.)
├── DuoAuthCore\            # Projet API .NET Core
│   ├── appsettings.json    # Configuration application
├── README.md               # Ce fichier
└── ...
```

***

## 🔗 Ressources utiles

- [Documentation officielle Duo Universal Prompt](https://duo.com/docs/duoweb)
- [Exemple Duo Universal C# GitHub](https://github.com/duosecurity/duo_universal_csharp)
- [Documentation Middleware Session ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-9.0)

***

## 🙋 Questions / Contribution

Toute contribution, question ou bug est la bienvenue. Merci de bien détailler vos scénarios et erreurs rencontrées lors de la soumission d’issues ou pull requests.

***

Avec ce README, vos utilisateurs et développeurs disposeront d’une vue claire, structurée et complète sur comment déployer, tester et maintenir l’intégration Duo avec ASP Classic et .NET Core dans ce projet.

[1](https://duo.com/docs/duoweb)
[2](https://github.com/duosecurity/duo_universal_nodejs/blob/main/example/README.md)
[3](https://github.com/duosecurity/duo_universal_java)
[4](https://docs.github.com/en/authentication/securing-your-account-with-two-factor-authentication-2fa/configuring-two-factor-authentication)
[5](https://github.com/duosecurity)
[6](https://gist.github.com/martensonbj/6bf2ec2ed55f5be723415ea73c4557c4)
[7](https://github.com/duosecurity/duo_java)
[8](https://github.com/duosecurity/duo_universal_csharp)
[9](https://www.youtube.com/watch?v=eVGEea7adDM)
[10](https://dev.to/sumonta056/github-readme-template-for-personal-projects-3lka)
Comment déployer, tester et maintenir l’intégration Duo avec ASP Classic et .NET Core dans ce projet.
