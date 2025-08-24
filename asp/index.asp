<%
' Page d'accueil après authentification Duo MFA réussie
' (Aucune logique serveur ici, juste une page de bienvenue)
%>
<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="utf-8" />
    <title>Bienvenue - Authentification Duo Réussie</title>
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <style>
        body {
            min-height: 100vh;
            background: linear-gradient(135deg, #e0e7ff 0%, #f8fafc 100%);
            display: flex;
            align-items: center;
            justify-content: center;
            font-family: 'Segoe UI', Arial, sans-serif;
            margin: 0;
        }
        .glass {
            background: rgba(255, 255, 255, 0.35);
            box-shadow: 0 8px 32px 0 rgba(31, 38, 135, 0.18);
            backdrop-filter: blur(10px);
            -webkit-backdrop-filter: blur(10px);
            border-radius: 18px;
            border: 1.5px solid rgba(255, 255, 255, 0.25);
            padding: 40px 32px;
            max-width: 420px;
            width: 100%;
            text-align: center;
        }
        h1 {
            color: #2563eb;
            margin-bottom: 18px;
            font-size: 2rem;
        }
        .subtitle {
            color: #0f172a;
            font-size: 1.1rem;
            margin-bottom: 24px;
        }
        .info {
            background: rgba(37, 99, 235, 0.08);
            color: #1e293b;
            border-radius: 8px;
            padding: 14px 10px;
            margin-bottom: 18px;
            font-size: 1rem;
        }
        .actions {
            margin-top: 24px;
        }
        .actions a {
            display: inline-block;
            margin: 0 10px;
            color: #2563eb;
            text-decoration: none;
            font-weight: 500;
            transition: color 0.2s;
        }
        .actions a:hover {
            color: #1e40af;
            text-decoration: underline;
        }
    </style>
</head>
<body>
    <div class="glass">
        <h1>🎉 Bienvenue !</h1>
        <div class="subtitle">
            Vous avez survécu à l'authentification Duo MFA.<br>
            Accès sécurisé accordé.
        </div>
        <div class="info">
            <strong>Informations Génériques :</strong><br>
            <ul style="text-align:left; margin: 12px 0 0 0; padding-left: 18px;">
                <li>Authentification à deux facteurs réussie</li>
                <li>Session sécurisée active</li>
                <li>Vous pouvez maintenant accéder aux ressources protégées</li>
            </ul>
        </div>
        <div class="actions">
            <a href="protected.asp">Accéder à la page protégée</a>
            <a href="logout.asp">Se déconnecter</a>
        </div>
    </div>
</body>
</html>