# 🚀 Démarrage Rapide - Intégration Duo

## ⚡ Test Rapide en 5 Minutes

### **1. Démarrer l'Application .NET Core**

#### **📁 Terminal 1 :**
```bash
cd C:\inetpub\wwwroot\asp\DuoAuthCore
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --urls="http://localhost:5000"
```

#### **✅ Vérification :**
```bash
# Tester l'endpoint de santé
curl http://localhost:5000/api/health

# Réponse attendue :
# {"status":"OK","environment":"Development","timestamp":"2025-08-24T...","version":"1.0.0"}
```

### **2. Tester l'Application ASP Classic**

#### **🌐 Navigateur :**
- **URL** : `http://localhost/asp/login.asp`
- **Compte test** : `admin` / `password123`

#### **🔄 Flux attendu :**
1. **Connexion** → Validation des credentials ✅
2. **Redirection** → `http://localhost:5000/duo-auth?username=admin&returnUrl=...` ✅
3. **Duo Auth** → Initialisation de session ✅
4. **Redirection Duo** → URL Duo générée ✅
5. **Callback** → Retour vers ASP Classic ✅
6. **Validation** → Token reçu et validé ✅
7. **Accès** → Page protégée accessible ✅

---

## 🔍 Débogage en Temps Réel

### **📊 Logs .NET Core (Terminal 1) :**
```bash
info: DuoAuthCore.Controllers.DuoAuthController[0]
      Début authentification Duo pour admin
info: DuoAuthCore.Controllers.DuoAuthController[0]
      Session initialisée - ID: abc123, State: xyz789
info: DuoAuthCore.Controllers.DuoAuthController[0]
      Redirection vers Duo: https://api-xxx.duosecurity.com/...
```

### **📊 Logs ASP Classic :**
```asp
<!-- Ajouter dans login.asp pour le débogage -->
<%
If method = "POST" Then
    Response.Write "<div style='background:#f0f0f0; padding:10px; margin:10px;'>"
    Response.Write "<strong>DEBUG:</strong><br>"
    Response.Write "Username: " & username & "<br>"
    Response.Write "DuoAuthUrl: " & duoAuthUrl & "<br>"
    Response.Write "</div>"
    Response.Flush
End If
%>
```

---

## 🚨 Résolution des Problèmes Courants

### **❌ Port 5000 déjà utilisé :**
```bash
# Solution 1 : Libérer le port
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Solution 2 : Changer le port
dotnet run --urls="http://localhost:5001"
```

### **❌ Erreur de session :**
```bash
# Vérifier la configuration des sessions
# Dans Program.cs, s'assurer que AddSession() est appelé
```

### **❌ Erreur CSRF :**
```csharp
// Vérifier la génération de l'état
var state = GenerateSecureState();
HttpContext.Session.SetString("duo_state", state);
```

---

## 📋 Checklist de Test Rapide

### **✅ Phase 1 - Démarrage :**
- [ ] Application .NET Core démarrée sur le port 5000
- [ ] Endpoint `/api/health` accessible
- [ ] Environnement affiché : "Development"

### **✅ Phase 2 - Connexion :**
- [ ] Page `login.asp` accessible
- [ ] Formulaire de connexion fonctionnel
- [ ] Validation des credentials réussie

### **✅ Phase 3 - Redirection :**
- [ ] Redirection vers .NET Core réussie
- [ ] Session initialisée
- [ ] État CSRF généré

### **✅ Phase 4 - Duo :**
- [ ] URL Duo générée
- [ ] Redirection vers Duo
- [ ] Callback traité

### **✅ Phase 5 - Retour :**
- [ ] Retour vers ASP Classic
- [ ] Token reçu et validé
- [ ] Accès à la page protégée

---

## 🎯 Test Complet du Flux

### **1. Test de bout en bout :**
```bash
# 1. Démarrer l'application
cd DuoAuthCore && dotnet run

# 2. Ouvrir le navigateur
# Accéder à http://localhost/asp/login.asp

# 3. Se connecter avec admin/password123

# 4. Suivre le flux complet
# 5. Vérifier l'accès à protected.asp
```

### **2. Vérification des logs :**
```bash
# Terminal 1 : Logs .NET Core
# Terminal 2 : Logs IIS (Observateur d'événements)
# Navigateur : Débogage ASP Classic
```

### **3. Validation finale :**
- ✅ Utilisateur connecté
- ✅ Session Duo créée
- ✅ Token JWT généré
- ✅ Accès protégé accordé

---

## 🔧 Configuration Avancée

### **Mode Développement :**
```bash
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --urls="http://localhost:5000"
```

### **Mode Production :**
```bash
set ASPNETCORE_ENVIRONMENT=Production
dotnet run --urls="http://localhost:5000"
```

### **Port personnalisé :**
```bash
dotnet run --urls="http://localhost:5001"
```

---

## 💡 Conseils de Débogage

### **1. Commencer simple :**
- Testez d'abord l'endpoint de santé
- Puis le formulaire de connexion
- Enfin le flux complet

### **2. Surveiller les logs :**
- Console .NET Core pour les erreurs
- Navigateur pour les redirections
- Observateur d'événements IIS

### **3. Vérifier les URLs :**
- Ports corrects (80 pour ASP, 5000 pour Core)
- Paramètres de requête valides
- Redirections fonctionnelles

---

**🎯 Objectif :** Avoir un flux d'authentification complet fonctionnel en moins de 10 minutes !

**🚀 Prêt à tester ?** Suivez les étapes ci-dessus et vous devriez avoir une intégration Duo fonctionnelle rapidement !
