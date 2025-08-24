# 🚀 Mode Opératoire Complet d'Intégration Duo

## 🎯 Vue d'ensemble du Flux d'Authentification

```
ASP Classic login.asp
    ↓
→ POST avec username/password
    ↓
→ Validation des credentials
    ↓
→ Redirection vers http://localhost:5000/duo-auth?username=xxx&returnUrl=xxx
    ↓
→ Votre API Core (/duo-auth) reçoit la requête
    ↓
→ Initialise la session et génère l'état
    ↓
→ Redirige directement vers Duo
    ↓
→ L'utilisateur s'authentifie sur Duo
    ↓
→ Duo redirige vers /api/duoauth/callback
    ↓
→ Core traite le callback et redirige vers duo_callback.asp
    ↓
→ ASP Classic continue son processus
```

## 🔧 Configuration Initiale

### **1. Prérequis Système**

#### **✅ Vérifications obligatoires :**
```powershell
# Vérifier que IIS est démarré
Get-Service -Name W3SVC

# Vérifier que le port 5000 est libre
netstat -ano | findstr :5000

# Vérifier les permissions sur le dossier
icacls "C:\inetpub\wwwroot\asp"
```

#### **✅ Composants requis :**
- IIS avec support ASP Classic activé
- .NET Core 6.0+ installé
- Duo Security Developer Account
- Visual Studio Code (recommandé)

### **2. Configuration des Variables d'Environnement**

#### **🔑 Fichier `duo_config.txt` (ASP Classic) :**
```txt
DUO_CLIENT_ID=DIQ8BOPVUCELV4V4S4H0
DUO_CLIENT_SECRET=G702NyF96D1eKge9AbdrVJOb7o7IVJ19B6xh0E5I
DUO_API_HOST=api-a322db2c.duosecurity.com
DUO_REDIRECT_URI=http://localhost:5000/api/duoauth/callback
```

#### **🔑 Fichier `DuoAuthCore/appsettings.json` :**
```json
{
  "DuoSecurity": {
    "ClientId": "DIQ8BOPVUCELV4V4S4H0",
    "ClientSecret": "G702NyF96D1eKge9AbdrVJOb7o7IVJ19B6xh0E5I",
    "ApiHost": "api-a322db2c.duosecurity.com",
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

## 🚀 Démarrage et Test

### **1. Démarrer l'Application .NET Core**

#### **📁 Terminal 1 - Application Core :**
```bash
cd C:\inetpub\wwwroot\asp\DuoAuthCore
dotnet restore
dotnet run --urls="http://localhost:5000"
```

#### **✅ Vérification du démarrage :**
```bash
# Tester l'endpoint de santé
curl http://localhost:5000/api/health

# Vérifier que l'app écoute sur le port 5000
netstat -ano | findstr :5000
```

### **2. Tester l'Application ASP Classic**

#### **🌐 Accès via navigateur :**
- **URL principale** : `http://localhost/asp/`
- **Page de connexion** : `http://localhost/asp/login.asp`
- **Page protégée** : `http://localhost/asp/protected.asp`

#### **✅ Comptes de test disponibles :**
- **Admin** : `admin` / `password123`
- **Utilisateur** : `user` / `user123`
