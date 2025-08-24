🎯 RÉSUMÉ DE VOTRE ARCHITECTURE :

Utilisateur 
    → http://localhost/asp/login.asp (ASP Classic) 
    → Redirection → http://localhost:5000/duo-auth (ASP.NET Core + Duo)
    → Authentification Duo 
    → Redirection → http://localhost/asp/duo_callback.asp (ASP Classic)
    → Accès accordé

# Intégration Duo Web SDK v4 pour ASP Classic

Cette implémentation suit les recommandations officielles de Duo pour l'authentification à deux facteurs.

## Architecture

### Flux d'authentification recommandé par Duo

1. **Génération du state** : Le client génère une valeur d'état unique associée à l'utilisateur
2. **URL de requête** : Génération de l'URL d'autorisation Duo avec redirection
3. **Callback** : Endpoint qui reçoit le code + state de Duo après 2FA
4. **Échange de token** : Échange du code d'autorisation contre un token d'accès

### Fichiers créés

- `includes/duo_auth.asp` - Fonctions d'intégration Duo
- `duo_auth.asp` - Page d'initiation de l'authentification Duo
- `duo_callback.asp` - Gestion du retour OAuth de Duo
- `duo_config.txt` - Instructions de configuration
- `README_DUO.md` - Documentation complète

## Configuration

### 1. Credentials Duo

Modifiez `includes/duo_auth.asp` avec vos informations :

```asp
Const DUO_CLIENT_ID = "votre_client_id_duo"
Const DUO_CLIENT_SECRET = "votre_client_secret_duo"
Const DUO_API_HOST = "api-xxxxxxxx.duosecurity.com"
Const DUO_REDIRECT_URI = "http://votredomaine.com/duo_callback.asp"
```

### 2. Console Duo Admin

- Applications > Protect an application
- Sélectionnez "Web SDK"
- Ajoutez l'URL de redirection : `http://votredomaine.com/duo_callback.asp`

## Sécurité

### Protection CSRF
- Génération de state unique par session
- Validation du state dans le callback
- Nettoyage automatique après utilisation

### Gestion des sessions
- Sessions ASP classiques avec timeout
- Nettoyage complet lors de la déconnexion
- Validation de l'état d'authentification

## Utilisation

### Flux utilisateur

1. **Connexion initiale** : `login.asp` avec username/password
2. **Redirection Duo** : `duo_auth.asp` génère l'URL d'autorisation
3. **Authentification Duo** : L'utilisateur s'authentifie sur Duo
4. **Callback** : `duo_callback.asp` traite la réponse et valide
5. **Accès** : Redirection vers la page protégée

### Protection des pages

Pour protéger une page avec Duo 2FA :

```asp
<!--#include file="includes/auth.asp" -->
<%
RequireAuth ' Vérifie auth + Duo 2FA
%>
```

## Dépannage

### Erreurs communes

- **"Variable non définie"** : Vérifiez l'ordre des includes
- **"Erreur de compilation"** : Assurez-vous que les dossiers existent
- **"Erreur de sécurité"** : Le state a expiré, reconnectez-vous

### Debugging

Utilisez HAR (HTTP Archive) pour analyser le flux :
- Ouvrez les outils de développement du navigateur
- Onglet Network > Preserve log
- Suivez les redirections et appels API

## Conformité Duo

✅ **Pas d'iFrame** - Redirection directe vers Duo  
✅ **State parameter** - Protection CSRF  
✅ **OAuth 2.0** - Flux standard recommandé  
✅ **Gestion d'erreurs** - Validation et fallback  
✅ **Sécurité des sessions** - Nettoyage et validation  

## Support

Cette implémentation suit les recommandations officielles de Duo. Pour le support technique de Duo, consultez :
- [Documentation Duo Web SDK](https://duo.com/docs/duoweb)
- [KB Algorithmes JWT](https://help.duo.com/s/article/6830)
- [Support Duo](https://help.duo.com/) 