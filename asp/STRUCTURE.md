# 📁 Structure du Projet ASP Classic + Duo


## 🎯 Vue d'ensemble

Ce projet utilise une **architecture hybride** combinant ASP Classic et ASP.NET Core pour l'authentification Duo.

## 📂 Organisation des Fichiers

### **🏠 Racine du Projet (`C:\inetpub\wwwroot\asp\`)**

#### **Pages ASP Classic (Port 80)**
- `login.asp` → **Point d'entrée principal** - Gère la connexion initiale
- `duo_callback.asp` → **Gestion du retour** après authentification Duo
- `protected.asp` → **Page sécurisée** accessible après authentification
- `logout.asp` → **Déconnexion** et nettoyage des sessions
- `index.html` → **Page d'accueil** statique

#### **Configuration**
- `web.config` → **Configuration IIS** pour ASP Classic
- `duo_config.txt` → **Paramètres Duo** (clés, URLs, etc.)

### **📁 Dossier `includes/` - Fonctions Réutilisables**

#### **`auth.asp`**
- Fonctions d'authentification de base
- Gestion des sessions utilisateur
- Validation des droits d'accès

#### **`duo_auth.asp`**
- **FONCTIONS PRINCIPALES DUO** (ne pas supprimer !)
- `GenerateDuoState()` - Protection CSRF
- `GenerateDuoAuthURL()` - Génération URL d'auth
- `ExchangeCodeForToken()` - Échange code/token
- `ValidateDuoAuth()` - Validation de l'authentification

### **📁 Dossier `DuoAuthCore/` - Application .NET Core (Port 5000)**

#### **`appsettings.json`**
- Configuration de l'application .NET Core
- Paramètres de connexion Duo
- URLs et endpoints

### **📁 Dossier `data/` - Données**

#### **`users.txt`**
- Liste des utilisateurs autorisés
- Format simple pour tests

## 🔄 Flux d'Authentification

```
1. Utilisateur → login.asp (Port 80)
2. Redirection → DuoAuthCore (Port 5000)
3. Authentification → Service Duo
4. Callback → duo_callback.asp (Port 80)
5. Validation → Accès accordé
```

## ⚠️ **IMPORTANT : Fichiers à NE JAMAIS supprimer**

### **❌ Fichiers supprimés (confusion résolue)**
- ~~`duo_auth.asp` (racine)~~ → **DOUBLON** avec `includes/duo_auth.asp`
- ~~`READMEN.md`~~ → **FAUTE DE FRAÎPE**
- ~~`GUIDE_DEPLOIEMENT_DUO.md`~~ → **FICHIER VIDE**
- ~~`test_com.asp`~~ → **FICHIER DE TEST**

### **✅ Fichiers essentiels à conserver**
- `includes/duo_auth.asp` → **FONCTIONS DUO PRINCIPALES**
- `includes/auth.asp` → **AUTHENTIFICATION DE BASE**
- `duo_callback.asp` → **GESTION CALLBACK**
- `login.asp` → **POINT D'ENTRÉE**

## 🚀 Démarrage

### **1. Démarrer .NET Core :**
```bash
cd DuoAuthCore
dotnet run --urls="http://localhost:5000"
```

### **2. Accéder à l'application :**
- **ASP Classic** : `http://localhost/asp/`
- **.NET Core** : `http://localhost:5000`

## 🔧 Maintenance

### **Ajout de nouvelles fonctionnalités :**
- **Fonctions ASP Classic** → `includes/`
- **Pages utilisateur** → **Racine**
- **API .NET Core** → `DuoAuthCore/`

### **Tests :**
- Testez d'abord sur `localhost:5000`
- Puis intégrez avec ASP Classic
- Vérifiez les logs IIS

## 📚 Documentation

- **`README.md`** → Vue d'ensemble et démarrage
- **`README_DUO.md`** → Détails techniques Duo
- **`STRUCTURE.md`** → Ce fichier (organisation)

---

**💡 Conseil :** Gardez cette structure simple et logique. Évitez les doublons et les fichiers de test en production.
