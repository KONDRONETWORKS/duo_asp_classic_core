# 📋 Mode Opératoire d'Intégration Duo MFA avec ASP Classic

## 🎯 Objectif
Intégrer l'authentification multi-facteur (MFA) Duo Universal SDK dans une application ASP Classic existante en utilisant une application bridge ASP.NET Core.

## 📋 Prérequis
- ✅ Application ASP Classic fonctionnelle
- ✅ Compte Duo Security avec application configurée
- ✅ .NET 6.0 SDK installé
- ✅ IIS avec support ASP Classic et .NET Core

## 🔧 Configuration Duo Admin Panel

### 1. Création de l'Application dans Duo
- Aller dans le Duo Admin Panel
- **Applications** → **Protect an Application**
- Trouver **"Universal Prompt"** → **Protect this Application**
- Configurer:
  - **Name**: `Votre App ASP Classic`
  - **Type**: `Web SDK`
  - **RedirectUris**: `http://localhost:5000/Callback`

### 2. Récupération des Credentials
- **ClientId**: `DIQ8BOPVUCELV4V4S4H0` (20 caractères)
- **ClientSecret**: `G702NyF96D1eKge9AbdrVJOb7o7IVJ19B6xh0E5I`
- **ApiHostname**: `api-a322db2c.duosecurity.com`

## 🚀 Installation et Déploiement

### 1. Structure des Fichiers
```
C:\inetpub\wwwroot\
├── maran\                    # Application ASP Classic
│   ├── includes\
│   │   ├── auth.asp         # Modifié pour Duo
│   │   └── duo_auth.asp     # Nouveau
│   ├── login.asp            # Modifié
│   ├── duo_callback.asp     # Nouveau
│   └── web.config
└── DuoAuthCore\             # Application Bridge .NET
    ├── appsettings.json
    ├── Pages\
    │   ├── Index.cshtml.cs
    │   ├── Login.cshtml.cs
    │   └── Callback.cshtml.cs
    ├── Services\
    │   └── DuoClientProvider.cs
    └── Startup.cs
```

### 2. Configuration ASP.NET Core
**appsettings.json**
```json
{
  "Duo": {
    "ClientId": "DIQ8BOPVUCELV4V4S4H0",
    "ClientSecret": "G702NyF96D1eKge9AbdrVJOb7o7IVJ19B6xh0E5I",
    "ApiHost": "api-a322db2c.duosecurity.com",
    "RedirectUri": "http://localhost:5000/Callback",
    "Failmode": "closed"
  }
}
```

### 3. Modifications ASP Classic

**includes/auth.asp** - Ajouter:
```asp
Function IsDuoAuthenticated()
    IsDuoAuthenticated = (Session("DuoAuthenticated") = "true")
End Function
```

**login.asp** - Modifier la redirection:
```asp
<%
duoAuthUrl = "http://localhost:5000/duo-auth?returnUrl=" & Server.URLEncode(returnUrl)
Response.Redirect(duoAuthUrl)
%>
```

### 4. Démarrage de l'Application Bridge
```powershell
cd C:\inetpub\wwwroot\DuoAuthCore
dotnet build -c Release
dotnet run --urls="http://localhost:5000"
```

## 🔄 Flux d'Authentification

### 1. Utilisateur accède à l'app
```
GET http://localhost/maran/login.asp
```

### 2. Authentification Classique
- Validation credentials dans `login.asp`
- Redirection vers bridge .NET

### 3. Initialisation Duo
```
GET http://localhost:5000/duo-auth?returnUrl=[callback]
```

### 4. Redirection vers Duo
- Génération de l'URL Duo
- Redirection vers `api-a322db2c.duosecurity.com`

### 5. Callback et Validation
```
GET http://localhost:5000/Callback?code=[code]&state=[state]
```

### 6. Retour vers ASP Classic
```
GET http://localhost/maran/duo_callback.asp?token=[jwt]&username=[user]
```

## 🧪 Tests de Validation

### 1. Test de l'Application Bridge
```powershell
# Test du endpoint principal
curl http://localhost:5000/

# Test de santé de l'API
curl http://localhost:5000/api/health
```

### 2. Test ASP Classic
```powershell
# Test de la page de login
curl http://localhost/maran/login.asp

# Test de la page protégée
curl http://localhost/maran/protected.asp
```

### 3. Test d'Intégration Complet
1. Ouvrir `http://localhost/maran/login.asp`
2. S'authentifier avec credentials
3. Compléter l'authentification Duo
4. Vérifier l'accès aux pages protégées

## 🔧 Dépannage

### Erreur Courante: ClientId invalide
**Symptôme**: `ClientId must be a non-empty string of length 20`
**Solution**: Vérifier la configuration dans `appsettings.json`

### Erreur: Session corrompue
**Symptôme**: `Error unprotecting the session cookie`
**Solution**: 
```powershell
# Régénérer les clés de session
Remove-Item -Recurse -Force C:\temp-keys\*
```

### Erreur: Endpoint introuvable
**Symptôme**: `HTTP ERROR 404`
**Solution**: Vérifier que l'application .NET est démarrée sur le port 5000

## 📊 Monitoring

### Logs ASP.NET Core
```powershell
# Voir les logs en temps réel
dotnet run --urls="http://localhost:5000"
```

### Vérification des Services
```powershell
# Vérifier les ports ouverts
netstat -ano | findstr :5000

# Vérifier les services IIS
Get-Service -Name W3SVC, WAS
```

## 🚀 Procédure de Mise en Production

### 1. Configuration Production
- Changer les URLs de `localhost` vers les domaines réels
- Mettre à jour les RedirectUris dans Duo Admin
- Configurer SSL/TLS

### 2. Déploiement
```powershell
# Publication de l'application
dotnet publish -c Release -o C:\inetpub\duo-bridge

# Configuration comme service Windows
sc create DuoBridge binPath="C:\inetpub\duo-bridge\DuoAuthCore.exe"
```

### 3. Sécurité
- Rotation régulière des ClientSecrets
- Monitoring des logs d'authentification
- Mise à jour régulière des packages NuGet

## 📞 Support

### Logs à Fournir en Cas de Problème
1. Logs de l'application .NET Core
2. Capture d'écran de l'erreur
3. Configuration Duo (masquer les secrets)
4. Requête HTTP failing

### Contacts
- Équipe Sécurité: security@example.com
- Support Duo: support@duo.com
- Développement: dev@example.com

---

**📋 Document Maintenu Par**: Équipe de Développement  
**🔄 Dernière Mise à Jour**: 21/08/2025  
**✅ Statut**: Implémentation Validée et Testée**