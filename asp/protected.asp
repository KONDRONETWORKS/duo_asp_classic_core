<!--#include file="includes/auth.asp" -->
<%
Response.Buffer = True
RequireAuth
%>
<!DOCTYPE html>
<html lang="fr">
<head>
	<meta charset="utf-8" />
	<title>Espace protégé</title>
	<style>
		body { font-family: Segoe UI, Arial, sans-serif; background:#f5f5f5; margin:0; padding:40px; }
		.container { max-width:720px; margin:0 auto; background:#fff; padding:24px; border-radius:8px; box-shadow:0 2px 12px rgba(0,0,0,0.08); }
		h1 { margin:0 0 16px; font-size:22px; }
		.actions a { display:inline-block; margin-right:16px; color:#0066cc; text-decoration:none; }
	</style>
</head>
<body>
	<div class="container">
		<h1>Espace protégé</h1>
		<p>Bonjour, <strong><%= Server.HTMLEncode(CurrentUsername()) %></strong> !</p>
		<div class="actions">
			<a href="logout.asp">Se déconnecter</a>
		</div>
		<p>Contenu uniquement accessible aux utilisateurs authentifiés.</p>
	</div>
</body>
</html> 