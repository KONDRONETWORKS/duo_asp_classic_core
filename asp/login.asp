<!--#include file="includes/auth.asp" -->

<%
Response.Buffer = True

Dim method, username, password, errorMessage, returnUrl, duoAuthUrl
method = UCase(Request.ServerVariables("REQUEST_METHOD"))
returnUrl = "http://localhost/asp/duo_callback.asp"

errorMessage = ""

If method = "POST" Then
    username = Trim(Request.Form("username"))
    password = CStr(Request.Form("password"))
    
    If ValidateCredentials(username, password) Then
        SignIn username
        
        ' ✅ CORRECTION: Utiliser l'endpoint /duo-auth qui gère directement la redirection
        duoAuthUrl = "http://localhost:5000/api/duoauth/duo-auth?username=" & Server.URLEncode(username) & _
                    "&returnUrl=" & Server.URLEncode(returnUrl)
        
        ' Rediriger vers ASP.NET Core pour lancer DIRECTEMENT Duo MFA
        Response.Redirect duoAuthUrl
        Response.End
    Else
        errorMessage = "Nom d'utilisateur ou mot de passe incorrect."
    End If
End If
%>

<!DOCTYPE html>
<html lang="fr">
<head>
	<meta charset="utf-8" />
	<title>Connexion</title>
	<style>
		body { font-family: Segoe UI, Arial, sans-serif; background:#f5f5f5; margin:0; padding:40px; }
		.container { max-width:420px; margin:0 auto; background:#fff; padding:24px; border-radius:8px; box-shadow:0 2px 12px rgba(0,0,0,0.08); }
		h1 { margin:0 0 16px; font-size:22px; }
		label { display:block; margin-top:12px; }
		input[type=text], input[type=password] { width:100%; padding:10px; margin-top:6px; box-sizing:border-box; }
		button { margin-top:16px; padding:10px 16px; background:#0066cc; color:#fff; border:none; border-radius:4px; cursor:pointer; }
		.error { color:#b00020; margin-top:12px; }
		.hint { color:#666; font-size:12px; margin-top:10px; }
	</style>
</head>
<body>
	<div class="container">
		<h1>Connexion</h1>
		<% If Len(errorMessage) > 0 Then %>
			<p class="error"><%= errorMessage %></p>
		<% End If %>
		<form method="post" action="login.asp">
			<input type="hidden" name="returnUrl" value="<%= Server.HTMLEncode(returnUrl) %>" />
			<label for="username">Nom d'utilisateur</label>
			<input type="text" id="username" name="username" value="<%= Server.HTMLEncode(username) %>" autocomplete="username" required />
			<label for="password">Mot de passe</label>
			<input type="password" id="password" name="password" autocomplete="current-password" required />
			<button type="submit">Se connecter</button>
			<p class="hint">Comptes de test: admin/password123, user/user123</p>
		</form>
	</div>
</body>
</html> 

' <%
' Response.Buffer = True

' Dim method, username, password, errorMessage, returnUrl, redirectUrl, duoAuthUrl
' method = UCase(Request.ServerVariables("REQUEST_METHOD"))
' returnUrl = Request("returnUrl")
' URL de retour après authentification Duo
' returnUrl = "http://localhost/asp/duo_callback.asp"

' URL de l'application ASP.NET Core Duo
' duoAuthUrl = "http://localhost:5000/duo-auth?username=" & Server.URLEncode(username) & _
' "&returnUrl=" & Server.URLEncode("http://localhost/asp/duo_callback.asp")
' errorMessage = ""

' If method = "POST" Then
' 	username = Trim(Request.Form("username"))
' 	password = CStr(Request.Form("password"))
' 	If ValidateCredentials(username, password) Then
' 		SignIn username
		
' 		' Rediriger vers ASP.NET Core pour gérer Duo		
' 		Response.Redirect duoAuthUrl
' 	End If
' End If
' %>