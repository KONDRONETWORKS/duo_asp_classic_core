# 🚀 Mode Opératoire - Configuration IIS & Déploiement

## 📋 SOMMAIRE
1. **Configuration IIS pour ASP Classic + .NET Core**
2. **Fichiers Web.Config Critiques**  
3. **Routing et URLs de Production**  
4. **Déploiement en Dev vs Production**  
5. **Monitoring et Troubleshooting**  

---

## 1. 🔧 CONFIGURATION IIS

### ✅ Prérequis IIS
```powershell
# Installation des fonctionnalités IIS
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASP
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET45
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpRedirect
```

### 📁 Structure des Sites IIS
```
Sites IIS/
├── 📁 Site Principal (Port 80)
│   └── 📁 Application: /maran (ASP Classic)
└── 📁 Application Bridge: DuoAuthCore (Port 5000 - Kestrel)
```

### 🔧 Configuration ASP Classic
**Application Pool** → .NET v4.0 + **Mode Intégré**

---

## 2. 📄 FICHIERS WEB.CONFIG CRITIQUES

### 🔹 Web.Config ASP Classic (C:\inetpub\wwwroot\maran\web.config)
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <clear />
      <add name="ASPClassic" 
           path="*.asp" 
           verb="*" 
           modules="IsapiModule" 
           scriptProcessor="%windir%\system32\inetsrv\asp.dll" 
           resourceType="File" />
    </handlers>
    
    <httpErrors errorMode="DetailedLocalOnly" />
    <asp enableParentPaths="true" scriptErrorSentToBrowser="true" />
    
  </system.webServer>
  
  <system.web>
    <customErrors mode="Off"/>
    <compilation debug="true"/>
    <httpRuntime maxRequestLength="4096" />
  </system.web>
</configuration>
```

### 🔹 Web.Config ASP.NET Core (C:\inetpub\wwwroot\DuoAuthCore\web.config)
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" 
           path="*" 
           verb="*" 
           modules="AspNetCoreModuleV2" 
           resourceType="Unspecified" />
    </handlers>
    
    <aspNetCore processPath="dotnet"
                arguments=".\DuoAuthCore.dll"
                stdoutLogEnabled="true"
                stdoutLogFile=".\logs\stdout"
                hostingModel="outofprocess" />
                
  </system.webServer>
</configuration>
```

---

## 3. 🌐 ROUTING & URLs EN PRODUCTION

### 🔄 Flux de Production
```
Utilisateur 
→ https://monapp.com/login.asp (ASP Classic) 
→ https://auth.monapp.com/duo-auth (ASP.NET Core) 
→ https://api-duo.com/oauth (Duo Cloud) 
→ https://auth.monapp.com/Callback (ASP.NET Core) 
→ https://monapp.com/duo_callback.asp (ASP Classic)
```

### 📝 Configuration Production ASP Classic
```asp
<%
' 🚨 EN DEVELOPMENT 
duoAuthUrl = "http://localhost:5000/duo-auth?returnUrl=" & returnUrl

' 🚀 EN PRODUCTION 
duoAuthUrl = "https://auth.monapp.com/duo-auth?returnUrl=" & returnUrl
%>
```

### 📝 Configuration Production ASP.NET Core
**appsettings.Production.json**
```json
{
  "Duo": {
    "ClientId": "VOTRE_CLIENT_ID_PROD",
    "ClientSecret": "VOTRE_SECRET_PROD", 
    "ApiHost": "api-XXXXXX.duosecurity.com",
    "RedirectUri": "https://auth.monapp.com/Callback"
  },
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:443",
        "Certificate": {
          "Path": "C:\\certificats\\monapp.pfx",
          "Password": "motdepasse"
        }
      }
    }
  }
}
```

---

## 4. 🏭 DÉPLOIEMENT DEV vs PRODUCTION

### 🔧 ENVIRONNEMENT DEVELOPMENT
```powershell
# Démarrage manuel
cd C:\inetpub\wwwroot\DuoAuthCore
dotnet run --urls="http://localhost:5000" --environment=Development

# Variables d'environnement
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ASPNETCORE_URLS="http://localhost:5000"
```

### 🚀 ENVIRONNEMENT PRODUCTION  
```powershell
# Publication
dotnet publish -c Release -o C:\inetpub\duo-bridge

# Installation comme Service Windows
sc create DuoBridge binPath="C:\inetpub\duo-bridge\DuoAuthCore.exe"
DisplayName= "Duo Authentication Bridge" start= auto

# Variables service
sc config DuoBridge Environment=ASPNETCORE_ENVIRONMENT=Production
```

### 📊 TABLEAU COMPARATIF DEV/PROD
| Paramètre          | Development         | Production         |
|--------------------|---------------------|-------------------|
| URL ASP Classic    | http://localhost/maran | https://monapp.com |
| URL .NET Core      | http://localhost:5000 | https://auth.monapp.com |
| Base URL Duo       | http://localhost:5000/Callback | https://auth.monapp.com/Callback |
| Logging            | Console + Debug     | Fichiers + ELK    |
| Certificat         | Self-signed         | Certificat SSL    |

---

## 5. 🔍 MONITORING & TROUBLESHOOTING

### 📊 Vérification Services
```powershell
# Statut services
Get-Service -Name W3SVC, WAS, DuoBridge

# Ports ouverts
netstat -ano | findstr ":80 :443 :5000"

# Logs temps réel
Get-Content C:\inetpub\wwwroot\DuoAuthCore\logs\stdout_*.log -Wait
```

### 🐛 Troubleshooting Commun
**Problème**: Erreur 500.19  
**Solution**: 
```powershell
# Réinitialisation IIS
iisreset /restart
```

**Problème**: Session corrompue  
**Solution**: 
```powershell
# Suppression keys corrompues
Remove-Item -Recurse -Force C:\temp-keys\*
```

**Problème**: Certificat SSL  
**Solution**:
```powershell
# Import certificat
Import-PfxCertificate -FilePath "C:\certificats\monapp.pfx" -CertStoreLocation Cert:\LocalMachine\My
```

### 📋 CHECKLIST DÉPLOIEMENT PRODUCTION
- [ ] Certificat SSL configuré
- [ ] URLs de production dans tous les fichiers
- [ ] Variables d'environnement définies
- [ ] Service Windows installé
- [ ] Redondance configurée
- [ ] Monitoring activé
- [ ] Backup des keys de chiffrement

---

## 🚨 URGENCE - ROLLBACK RAPIDE
```powershell
# Arrêt service
sc stop DuoBridge

# Retour version précédente
sc start DuoBridgePrevious

# Réactivation ASP Classic seul
# (Modifier web.config pour désactiver redirection Duo)
```

---

**📞 Support Urgent**: infrastructure@entreprise.com  
**🔒 Sécurité**: security@entreprise.com  
**📋 Documentation**: https://wiki.entreprise.com/duo-integration  

**🔄 Dernière Vérification**: 21/08/2025  
**✅ Environnement de Production**: VALIDÉ