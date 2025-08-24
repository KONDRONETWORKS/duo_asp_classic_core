# 🔧 Configuration Modes Développement et Production

## 🚀 Mode Développement (Dev)

### **Configuration `appsettings.Development.json` :**
```json
{
  "DuoSecurity": {
    "ClientId": "DEV_CLIENT_ID",
    "ClientSecret": "DEV_CLIENT_SECRET",
    "ApiHost": "api-dev.duosecurity.com",
    "RedirectUri": "http://localhost:5000/api/duoauth/callback"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "Environment": "Development",
  "Security": {
    "RequireHttps": false,
    "JwtExpirationMinutes": 60,
    "SessionTimeoutMinutes": 120
  }
}
```

### **Démarrage en mode Dev :**
```bash
cd DuoAuthCore
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --urls="http://localhost:5000"
```

### **Fonctionnalités Dev :**
- ✅ Logs détaillés et débogage
- ✅ Validation des tokens simplifiée
- ✅ Gestion d'erreurs verbeuse
- ✅ Comptes de test intégrés
- ✅ CORS ouvert pour le développement
- ✅ Pas de validation HTTPS stricte

---

## 🚀 Mode Production (Pro)

### **Configuration `appsettings.Production.json` :**
```json
{
  "DuoSecurity": {
    "ClientId": "${DUO_CLIENT_ID}",
    "ClientSecret": "${DUO_CLIENT_SECRET}",
    "ApiHost": "${DUO_API_HOST}",
    "RedirectUri": "${DUO_REDIRECT_URI}"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Environment": "Production",
  "Security": {
    "RequireHttps": true,
    "JwtExpirationMinutes": 30,
    "SessionTimeoutMinutes": 60
  }
}
```

### **Démarrage en mode Pro :**
```bash
cd DuoAuthCore
set ASPNETCORE_ENVIRONMENT=Production
dotnet run --urls="http://localhost:5000"
```

### **Fonctionnalités Pro :**
- ✅ Logs optimisés pour la production
- ✅ Validation stricte des tokens
- ✅ Gestion d'erreurs sécurisée
- ✅ Monitoring et alertes
- ✅ CORS restreint
- ✅ Validation HTTPS obligatoire

---

## 🔄 Basculement entre Modes

### **1. Vérifier l'environnement actuel :**
```bash
echo %ASPNETCORE_ENVIRONMENT%
```

### **2. Changer vers le mode Dev :**
```bash
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

### **3. Changer vers le mode Pro :**
```bash
set ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

### **4. Vérifier la configuration active :**
```bash
# L'application affichera l'environnement actif
curl http://localhost:5000/api/health
```

---

## 📊 Comparaison des Modes

| Fonctionnalité | Développement | Production |
|----------------|---------------|------------|
| **Logs** | Debug + Info | Warning + Error |
| **HTTPS** | Optionnel | Obligatoire |
| **CORS** | Ouvert | Restreint |
| **Sessions** | 120 min | 60 min |
| **JWT** | 60 min | 30 min |
| **Erreurs** | Détail complet | Messages génériques |
| **Performance** | Optimisé debug | Optimisé production |

---

## 🎯 Recommandations d'Usage

### **🔧 Développement :**
- Utiliser le mode Dev pour tous les tests
- Activer tous les logs de débogage
- Tester avec des comptes de développement
- Valider le flux complet avant passage en production

### **🚀 Production :**
- Tester en mode Dev avant déploiement
- Vérifier la configuration des variables d'environnement
- Monitorer les logs et performances
- Mettre en place des alertes de sécurité

---

**💡 Conseil :** Commencez toujours par le mode Développement pour tester et déboguer, puis passez en mode Production une fois que tout fonctionne correctement.
