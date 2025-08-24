<!--#include file="includes/auth.asp" -->
<!--#include file="includes/duo_auth.asp" -->
<%
Response.Buffer = True

' duo_callback.asp - Traite le retour après authentification Duo
Dim token, username, debug, isSuccess, tokenResult

' Initialisation
isSuccess = False
debug = ""

' Récupérer le token JWT et le username from .NET Core
token = Request.QueryString("token")
username = Request.QueryString("username")

' Ajout pour debug : afficher le token et username reçus
debug = "Token reçu: " & Server.HTMLEncode(token) & "<br/>" & _
        "Username reçu: " & Server.HTMLEncode(username) & "<br/>"

' Fonction locale pour vérifier format JWT basique
Function IsValidJwt(jwt)
    Dim parts
    parts = Split(jwt, ".")
    IsValidJwt = (UBound(parts) = 2) And (Len(jwt) > 20)
End Function

If token <> "" And username <> "" Then
    ' Valider que ce n'est pas l'objet lui-même (cas d'erreur connu) et que le token ait le format JWT
    If token <> "DuoUniversal.IdToken" And IsValidJwt(token) Then
        ' Token semble valide
        Session("Authenticated") = True
        Session("UserName") = username
        Session("DuoToken") = token
        isSuccess = True
        tokenResult = "Authentification réussie"
    Else
        ' Token invalide (objet au lieu du jeton ou format incorrect)
        isSuccess = False
        tokenResult = "Token invalide reçu : valeur brute '" & Server.HTMLEncode(token) & "' ou format JWT incorrect."
    End If
Else
    ' Paramètres manquants dans la requête
    isSuccess = False
    tokenResult = "Échec d'authentification : Token ou username manquant"
End If
%>
<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="utf-8" />
    <title>Authentification Duo - Résultat</title>
    <style>
        body { font-family: Segoe UI, Arial, sans-serif; background:#f5f5f5; margin:0; padding:40px; }
        .container { max-width:600px; margin:0 auto; background:#fff; padding:24px; border-radius:8px; box-shadow:0 2px 12px rgba(0,0,0,0.08); }
        h1 { margin:0 0 16px; font-size:22px; color:#0066cc; }
        .success { color:#28a745; background:#d4edda; padding:12px; border-radius:4px; margin:16px 0; }
        .error { color:#dc3545; background:#f8d7da; padding:12px; border-radius:4px; margin:16px 0; }
        .info { background:#d1ecf1; padding:12px; border-radius:4px; margin:16px 0; font-family: Consolas, monospace; font-size: 12px; white-space: pre-wrap; }
        .actions { margin-top:20px; }
        .actions a { display:inline-block; margin-right:16px; color:#0066cc; text-decoration:none; }
    </style>
</head>
<body>
    <div class="container">
        <h1>Authentification Duo - Résultat</h1>
        
            <div class="error">
                <h3>Erreur d'authentification</h3>
                <p><%= Server.HTMLEncode(tokenResult) %></p>
            </div>
        <% If isSuccess Then %>
            
        
        <% Else %>
            <div class="success">
                <h3>Authentification réussie !</h3>
                <p>Votre authentification à deux facteurs avec Duo a été complétée avec succès.</p>
            </div>
            
            <div class="info">
                <p><strong>Utilisateur :</strong> <%= Server.HTMLEncode(username) %></p>
                <p>Redirection en cours...</p>
            </div>
            
            <script type="text/javascript">
                // Redirection après 3 secondes vers la page protégée
                setTimeout(function(){
                    window.location.href = "index.asp";
                }, 3000);
            </script>
            <div class="info">
                <h4>Informations de diagnostic :</h4>
                <p><%= debug %></p>
            </div>
            
            <div class="actions">
                <a href="duo_auth.asp">Réessayer l'authentification Duo</a>
                <a href="login.asp">Retour à la page de connexion</a>
                <a href="logout.asp">Se déconnecter</a>
            </div>
        <% End If %>
    </div>
</body>
</html>
