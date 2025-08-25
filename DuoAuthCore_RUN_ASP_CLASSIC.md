# Guide d'Intégration Duo MFA avec Application ASP Classic

## Vue d'ensemble

Cette application .NET Core sert de pont entre votre application ASP Classic legacy et l'authentification Duo Universal Prompt. Elle gère le processus MFA et redirige l'utilisateur vers votre application ASP Classic une fois l'authentification réussie.

## Flux d'Authentification

1. **Authentification initiale** : L'utilisateur s'authentifie dans votre application ASP Classic avec username/password
2. **Redirection vers Duo** : Votre ASP Classic redirige vers cette application pour l'authentification MFA
3. **Authentification Duo** : L'utilisateur complète l'authentification MFA via Duo Universal Prompt
4. **Retour vers ASP Classic** : Après authentification réussie, l'utilisateur est redirigé vers votre application

## Intégration avec ASP Classic

### Option 1 : Redirection directe vers la page d'initialisation

```asp
<%
' Après authentification username/password réussie
Dim username
username = Request.Form("username")

' Rediriger vers l'application Duo
Response.Redirect "https://localhost/DuoAuthCore/DuoInit?username=" & Server.URLEncode(username)
%>
```

### Option 2 : Utilisation de l'API REST

```asp
<%
' Appel de l'API pour initialiser l'authentification
Dim username
username = Request.Form("username")

' Créer une requête HTTP vers l'API
Dim xmlhttp
Set xmlhttp = Server.CreateObject("MSXML2.ServerXMLHTTP")
xmlhttp.Open "POST", "https://localhost/DuoAuthCore/api/auth/init", False
xmlhttp.setRequestHeader "Content-Type", "application/json"
xmlhttp.Send "{""username"": """ & username & """}"

If xmlhttp.Status = 200 Then
    ' Rediriger vers Duo
    Response.Redirect "https://localhost/DuoAuthCore/login"
Else
    Response.Write "Erreur d'initialisation"
End If
%>
```

### Vérification de l'état d'authentification

```asp
<%
' Vérifier si l'utilisateur est authentifié via Duo
Dim username
username = Session("username")

' Appel de l'API pour vérifier l'authentification
Dim xmlhttp
Set xmlhttp = Server.CreateObject("MSXML2.ServerXMLHTTP")
xmlhttp.Open "GET", "https://localhost/DuoAuthCore/api/auth/check?username=" & Server.URLEncode(username), False
xmlhttp.Send

If xmlhttp.Status = 200 Then
    Dim response
    response = xmlhttp.responseText
    
    ' Parser la réponse JSON (nécessite une bibliothèque JSON pour ASP Classic)
    ' Si Authenticated = true, l'utilisateur peut accéder aux ressources protégées
End If
%>
```

## Configuration

### appsettings.json

```json
{
  "Duo": {
    "ClientId": "VOTRE_CLIENT_ID",
    "ClientSecret": "VOTRE_CLIENT_SECRET",
    "ApiHost": "VOTRE_API_HOST.duosecurity.com",
    "RedirectUri": "https://localhost/DuoAuthCore/Callback",
    "Failmode": "closed"
  },
  "LegacyApp": {
    "RedirectUrl": "https://localhost/asp/protected.asp",
    "AuthCheckUrl": "https://localhost/asp/duo_auth.asp"
  }
}
```

### Points d'API disponibles

- **POST /api/auth/init** : Initialise l'authentification pour un utilisateur
- **GET /api/auth/check?username={username}** : Vérifie l'état d'authentification
- **POST /api/auth/logout** : Déconnecte l'utilisateur

## Sécurité

- Les sessions sont gérées côté serveur avec des cookies sécurisés
- L'état d'authentification est vérifié à chaque requête
- Les tokens Duo sont stockés en session et non exposés au client
- Protection contre les attaques de rejeu via la vérification d'état

## Déploiement

1. **Compiler l'application** :
   ```bash
   dotnet build
   dotnet publish -c Release
   ```

2. **Configurer IIS** :
   - Créer un site web pointant vers le dossier publié
   - Configurer le pool d'applications .NET Core
   - Activer le module ASP.NET Core

3. **Configurer les certificats SSL** pour HTTPS

4. **Mettre à jour la configuration** avec vos paramètres Duo

## Dépannage

### Erreurs courantes

- **Erreur de build** : Vérifier que tous les packages NuGet sont installés
- **Erreur de session** : Vérifier que la gestion de session est activée
- **Erreur de redirection** : Vérifier les URLs dans la configuration

### Logs

Les logs sont disponibles dans :
- `logs\stdout` (configuré dans web.config)
- Event Viewer Windows pour les erreurs IIS

## Support

Pour toute question ou problème, consultez :
- La documentation Duo Universal Prompt
- Les logs de l'application
- La configuration IIS et .NET Core 




## RUN 

```bash
dotnet add package DuoUniversal --version 1.3.1
dotnet add package Microsoft.Extensions.Configuration --version 6.0.0
dotnet add package Microsoft.Extensions.Configuration.Json --version 6.0.0
dotnet add package System.Text.Json --version 6.0.0
```

🔄 Reconstruction Complète
powershell
```bash
# 1. Nettoyage
dotnet clean .\DuoAuthCore.csproj

# 2. Restauration
dotnet restore .\DuoAuthCore.csproj

# 3. Build Release
dotnet build .\DuoAuthCore.csproj -c Release

# 4. Publication (optionnel)
dotnet publish .\DuoAuthCore.csproj -c Release -o ./publish

# 5. RUN ON PORT 5000
cd C:\inetpub\wwwroot\DuoAuthCore
dotnet run --urls="https://localhost:5001;http://localhost:5000"
  dotnet run --urls="http://localhost:5000"
```

# 6. Corrigez le Problème de Session ASP.NET Core
Dans Startup.cs ou Program.cs, ajoutez :

csharp
```bash
services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\temp-keys\"))
    .SetApplicationName("DuoAuthCore");
  ```
Créez le dossier :

powershell
```bash
mkdir C:\temp-keys
```
🧪 Test des Nouveaux Endpoints
powershell
```bash
# Testez l'endpoint principal
curl http://localhost:5000/

# Testez le nouvel endpoint /duo-auth
curl "http://localhost:5000/duo-auth?returnUrl=http://localhost/asp/duo_callback.asp"
```

Votre application ASP.NET Core fonctionne parfaitement ! ✅

bash
curl http://localhost:5000
# ✅ StatusCode: 200 OK
# ✅ Page HTML retournée avec formulaire de login


# Testez à nouveau
curl http://localhost/asp/login.asp

🔍 Si l'erreur persiste
Vérifiez les configurations parentes :

powershell
```bash
# Vérifiez la configuration au niveau du site
Get-WebConfigurationProperty -PSPath 'IIS:\Sites\Default Web Site' -Filter "system.webServer/directoryBrowse" -Name "enabled"

# Désactivez directoryBrowse au niveau du site si nécessaire
Set-WebConfigurationProperty -PSPath 'IIS:\Sites\Default Web Site' -Filter "system.webServer/directoryBrowse" -Name "enabled" -Value $false
```

# 🎉 FÉLICITATIONS ! Tout fonctionne parfaitement ! ✅

## ✅ **STATUT ACTUEL :**

### 🟢 **ASP.NET Core** - **OPÉRATIONNEL**
- **URL :** `http://localhost:5000`
- **Statut :** ✅ Fonctionne parfaitement
- **Page :** Formulaire de login affiché

### 🟢 **ASP Classic** - **OPÉRATIONNEL** 
- **URL :** `http://localhost/maran/login.asp`
- **Statut :** ✅ Fonctionne parfaitement
- **Page :** Formulaire de connexion affiché (Code 200)

### 🟢 **IIS** - **OPÉRATIONNEL**
- **Services :** ✅ W3SVC et WAS démarrés
- **Port 80 :** ✅ En écoute
- **Configuration :** ✅ Optimisée

## 🚀 **PROCHAINES ÉTAPES :**

Maintenant que les deux applications fonctionnent, voici comment les faire communiquer :

### 1. **Modifier `login.asp` pour rediriger vers ASP.NET Core**

```asp
<%
' Dans login.asp - Ajoutez cette logique de redirection
Dim returnUrl, duoAuthUrl

' URL de retour après authentification Duo
returnUrl = "http://localhost/maran/duo_callback.asp"

' URL de l'application ASP.NET Core Duo
duoAuthUrl = "http://localhost:5000/duo-auth?returnUrl=" & Server.URLEncode(returnUrl)

' Redirection vers Duo Auth
Response.Redirect(duoAuthUrl)
Response.End
%>
```

### 2. **Créer `duo_callback.asp` pour traiter le retour**

```asp
<%
' duo_callback.asp - Traite le retour après authentification Duo
Dim token, userInfo

' Récupérer le token JWT from .NET Core
token = Request.QueryString("token")

If token <> "" Then
    ' Ici, vous devrez valider le token JWT avec votre API .NET Core
    ' Pour l'instant, simuler une authentification réussie
    Session("Authenticated") = True
    Session("UserName") = "UtilisateurDuo"
    Session("DuoToken") = token
    
    Response.Redirect("protected.asp")
Else
    Response.Redirect("login.asp?error=auth_failed")
End If
%>
```

### 3. **Tester la communication entre les applications**

```powershell```
# Testez la redirection
curl "http://localhost:5000/duo-auth?returnUrl=http://localhost/maran/duo_callback.asp"

# Vérifiez que ASP.NET Core peut appeler des APIs
curl http://localhost:5000/api/health
```

## 📋 **VÉRIFICATIONS FINALES :**

```powershell```
# Vérifiez que les deux services répondent
curl http://localhost:5000
curl http://localhost/maran/login.asp

# Vérifiez les ports ouverts
netstat -ano | findstr :5000
netstat -ano | findstr :80
```

## 🎯 **RÉSUMÉ DE VOTRE ARCHITECTURE :**

```
Utilisateur 
    → http://localhost/maran/login.asp (ASP Classic) 
    → Redirection → http://localhost:5000/duo-auth (ASP.NET Core + Duo)
    → Authentification Duo 
    → Redirection → http://localhost/maran/duo_callback.asp (ASP Classic)
    → Accès accordé
```

**Tout est maintenant en place pour l'intégration Duo MFA !** 🚀

Vos deux applications sont opérationnelles et prêtes à communiquer. Il ne reste plus qu'à implémenter la logique de redirection et de validation des tokens.
