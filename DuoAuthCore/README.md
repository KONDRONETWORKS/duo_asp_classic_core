# DuoAuthCore - Architecture Fluide

Application ASP.NET Core moderne pour l'authentification Duo avec une architecture fluide et optimisée.

## 🏗️ Architecture Cible Implémentée

```
C:\inetpub\wwwroot\DuoAuthCore
│   DuoAuthCore.sln                -> Solution .NET 6+
│   DuoAuthCore.csproj             -> Projet principal modernisé
│   Program.cs                      -> Point d'entrée minimal hosting
│
├── Controllers/
│   │   DuoAuthController.cs       -> Contrôleur principal d'authentification
│   │   HealthController.cs        -> Endpoints de santé et monitoring
│
├── Providers/
│   │   DuoClientProvider.cs       -> Fournit l'instance DuoClient configurée
│
├── Services/
│   │   TempAuthStorage.cs         -> Stockage temporaire thread-safe pour l'auth
│
├── appsettings.json               -> Configuration (Duo + JWT)
└── README.md                      -> Documentation
```

## 🚀 Fonctionnalités Principales

### ✅ Architecture Fluide
- **Minimal Hosting** : Utilisation du pattern .NET 6+ pour une configuration fluide
- **Dependency Injection** : Services organisés et injectés automatiquement
- **Middleware Pipeline** : Configuration claire et ordonnée des middlewares
- **Logging Structuré** : Logs structurés avec Serilog pour un monitoring efficace

### 🔐 Authentification Duo
- **Redirection Fluide** : Gestion transparente des redirections vers Duo
- **Session Management** : Gestion robuste des sessions avec nettoyage automatique
- **State Validation** : Validation sécurisée des états de session
- **Error Handling** : Gestion d'erreurs centralisée avec codes d'erreur

### 🎯 JWT & Tokens
- **Validation Automatique** : Validation des tokens JWT avec gestion d'erreurs
- **Extraction Intelligente** : Extraction automatique des tokens depuis les réponses Duo
- **Expiration Management** : Gestion automatique de l'expiration des tokens

### 📊 Monitoring & Santé
- **Health Checks** : Endpoints de santé pour le monitoring
- **Configuration Validation** : Vérification automatique de la configuration au démarrage
- **Performance Metrics** : Logs de performance et métriques d'utilisation

## 🛠️ Configuration

### Configuration Duo
```json
{
  "Duo": {
    "ClientId": "your-duo-client-id",
    "ClientSecret": "your-duo-client-secret",
    "ApiHost": "your-duo-api-host",
    "RedirectUri": "https://your-domain.com/api/duoauth/callback"
  }
}
```

### Configuration JWT
```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-jwt-key-here",
    "Issuer": "DuoAuthCore",
    "Audience": "DuoAuthCore",
    "ExpirationMinutes": 60
  }
}
```

## 🔄 Flux d'Authentification

1. **Initiation** : `GET /api/duoauth/duo-auth?username=user&returnUrl=...`
2. **Redirection Duo** : L'utilisateur est redirigé vers Duo
3. **Authentification** : L'utilisateur s'authentifie sur Duo
4. **Callback** : `GET /api/duoauth/callback?code=...&state=...`
5. **Validation** : Échange du code contre un token JWT
6. **Redirection Finale** : Retour vers l'application avec le token

## 📡 Endpoints API

### Authentification
- `GET /api/duoauth/duo-auth` - Initie l'authentification Duo
- `GET /api/duoauth/callback` - Callback après authentification Duo
- `POST /api/duoauth/validate-token` - Valide un token JWT

### Santé & Monitoring
- `GET /health` - Vérification de santé globale
- `GET /api/health` - Vérification de santé détaillée
- `GET /api/health/duo-config` - Vérification de la configuration Duo

## 🔧 Développement

### Prérequis
- .NET 6.0 ou supérieur
- Visual Studio 2022 ou VS Code
- Compte Duo avec API credentials

### Installation
```bash
# Cloner le projet
git clone <repository-url>
cd DuoAuthCore

# Restaurer les packages
dotnet restore

# Configurer appsettings.json avec vos credentials Duo
# Lancer l'application
dotnet run
```

### Tests
```bash
# Tests unitaires
dotnet test

# Tests d'intégration
dotnet test --filter Category=Integration
```

## 🚀 Déploiement

### IIS
- Publier l'application dans `C:\inetpub\wwwroot\DuoAuthCore`
- Configurer le pool d'applications .NET Core
- Vérifier les permissions sur le dossier `C:\temp-keys\`

### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY publish/ /app/
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "DuoAuthCore.dll"]
```

## 📈 Avantages de l'Architecture

### 🎯 Fluidité
- **Configuration Unifiée** : Tout dans `Program.cs` pour une vue d'ensemble claire
- **Services Organisés** : Séparation claire des responsabilités
- **Middleware Pipeline** : Configuration séquentielle et logique

### 🔒 Sécurité
- **Validation Automatique** : Vérification des tokens et sessions
- **Gestion d'Erreurs** : Codes d'erreur standardisés et sécurisés
- **Nettoyage Automatique** : Suppression des données expirées

### 📊 Observabilité
- **Logging Structuré** : Logs JSON pour une analyse facile
- **Health Checks** : Monitoring en temps réel de l'application
- **Métriques** : Suivi des performances et de l'utilisation

### 🚀 Performance
- **Minimal Hosting** : Démarrage rapide et configuration optimisée
- **Cache Distribué** : Gestion efficace des sessions
- **Async/Await** : Opérations asynchrones pour une meilleure réactivité

## 🤝 Support

Pour toute question ou problème :
1. Consultez la documentation des endpoints
2. Vérifiez les logs de l'application
3. Testez les health checks
4. Contactez l'équipe de développement

---

**DuoAuthCore** - Architecture fluide pour une authentification Duo moderne et performante.
