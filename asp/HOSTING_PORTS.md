# 🌐 Configuration des Ports - Projet ASP Classic + Duo

## 🎯 Vue d'ensemble des Ports

Votre projet utilise **deux ports différents** pour séparer les responsabilités :

### **Port 80 (HTTP) - ASP Classic**
- **URL** : `http://localhost/asp/`
- **Responsabilité** : Interface utilisateur et logique métier
- **Technologie** : ASP Classic via IIS

### **Port 5000 (HTTP) - ASP.NET Core**
- **URL** : `http://localhost:5000`
- **Responsabilité** : Authentification Duo et API
- **Technologie** : ASP.NET Core (Kestrel)

## 🔧 Configuration des Ports

### **1. Port 80 - IIS (ASP Classic)**

#### **Configuration automatique :**
- IIS écoute automatiquement sur le port 80
- Aucune configuration supplémentaire requise
- Accessible via `http://localhost/asp/`

#### **Vérification :**
```powershell
# Vérifier que IIS écoute sur le port 80
netstat -ano | findstr :80

# Vérifier le service IIS
Get-Service -Name W3SVC
```

### **2. Port 5000 - ASP.NET Core**

#### **Configuration dans DuoAuthCore :**
```bash
cd DuoAuthCore
dotnet run --urls="http://localhost:5000"
```

#### **Vérification :**
```powershell
# Vérifier que l'app .NET Core écoute sur le port 5000
netstat -ano | findstr :5000

# Tester l'accès
curl http://localhost:5000
```

## 🚫 **IMPORTANT : Éviter la Confusion des Ports**

### **❌ Ne PAS faire :**
- Essayer d'accéder à l'app .NET Core via `http://localhost/asp/`
- Configurer ASP Classic pour écouter sur le port 5000
- Mélanger les URLs des deux applications

### **✅ Faire :**
- **ASP Classic** → `http://localhost/asp/` (Port 80)
- **.NET Core** → `http://localhost:5000` (Port 5000)
- Garder les deux applications séparées

## 🔄 Communication entre les Ports

### **Flux d'authentification :**

```
1. Utilisateur → http://localhost/asp/login.asp (Port 80)
2. Redirection → http://localhost:5000/duo-auth (Port 5000)
3. Authentification Duo → Service externe
4. Callback → http://localhost/asp/duo_callback.asp (Port 80)
```

### **Code de redirection (login.asp) :**
```asp
<%
' Redirection vers l'app .NET Core sur le port 5000
Response.Redirect("http://localhost:5000/duo-auth?returnUrl=" & Server.URLEncode("/asp/duo_callback.asp"))
%>
```

## 🛠️ Dépannage des Ports

### **Problème : Port 5000 déjà utilisé**

#### **Solution 1 : Changer le port**
```bash
cd DuoAuthCore
dotnet run --urls="http://localhost:5001"
```

#### **Solution 2 : Libérer le port**
```powershell
# Identifier le processus utilisant le port 5000
netstat -ano | findstr :5000

# Arrêter le processus (remplacer PID par l'ID réel)
taskkill /PID <PID> /F
```

### **Problème : Port 80 inaccessible**

#### **Vérifications :**
```powershell
# Vérifier que IIS est démarré
Get-Service -Name W3SVC

# Vérifier les permissions
icacls "C:\inetpub\wwwroot\asp"

# Vérifier la configuration du site dans IIS
```

## 📋 Checklist de Configuration

### **✅ Port 80 (ASP Classic) :**
- [ ] IIS démarré et fonctionnel
- [ ] Site configuré dans IIS Manager
- [ ] Permissions sur le dossier `C:\inetpub\wwwroot\asp\`
- [ ] Accessible via `http://localhost/asp/`

### **✅ Port 5000 (.NET Core) :**
- [ ] Application .NET Core démarrée
- [ ] Port 5000 libre et accessible
- [ ] Configuration dans `DuoAuthCore/appsettings.json`
- [ ] Accessible via `http://localhost:5000`

### **✅ Communication :**
- [ ] Redirection fonctionnelle entre les ports
- [ ] Callback correctement configuré
- [ ] Pas d'erreurs de CORS ou de sécurité

## 🔍 Commandes de Vérification

```powershell
# Vérifier tous les ports écoutés
netstat -ano | findstr LISTENING

# Vérifier les services IIS
Get-Service -Name W3SVC, WAS

# Tester l'accès aux deux applications
Invoke-WebRequest -Uri "http://localhost/asp/" -UseBasicParsing
Invoke-WebRequest -Uri "http://localhost:5000" -UseBasicParsing
```

---

**💡 Conseil :** Gardez toujours en tête que vous avez **deux applications séparées** sur **deux ports différents**. Cela évite la confusion et facilite le débogage.
