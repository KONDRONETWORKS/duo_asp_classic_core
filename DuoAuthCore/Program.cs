// SPDX-FileCopyrightText: 2022 Cisco Systems, Inc. and/or its affiliates
//
// SPDX-License-Identifier: BSD-3-Clause

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DuoAuthCore.Services;
using DuoAuthCore.Providers;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Configuration des services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuration Duo Client Provider
builder.Services.AddSingleton<IDuoClientProvider, DuoClientProvider>();

// Configuration DataProtection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\temp-keys\"))
    .SetApplicationName("DuoAuthCore");

// Services de session et cache
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Mettre à Always en production avec HTTPS
    options.Cookie.Name = "DuoAuth.Session";
});

// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Services d'utilitaires
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<TempAuthStorage>();

// Configuration du logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Test de la configuration Duo au démarrage
try
{
    var duoProvider = app.Services.GetRequiredService<IDuoClientProvider>();
    var client = duoProvider.GetDuoClient();
    app.Logger.LogInformation("✅ Configuration Duo VALIDE");
}
catch (Exception ex)
{
    app.Logger.LogError("❌ Configuration Duo ERREUR: {Message}", ex.Message);
}

// Middleware pipeline
app.UseRouting();
app.UseCors("AllowAll");
app.UseSession();

// Middleware de debug de session
app.Use(async (context, next) =>
{
    try
    {
        var session = context.Session;
        if (session != null && session.IsAvailable)
        {
            app.Logger.LogDebug("Session ID: {SessionId}, Keys: {Keys}", 
                session.Id, string.Join(", ", session.Keys));
        }
        else
        {
            app.Logger.LogWarning("⚠️ La session n'est pas disponible pour cette requête.");
        }
    }
    catch (InvalidOperationException ex)
    {
        app.Logger.LogWarning("⚠️ Erreur session: {Message}", ex.Message);
    }
    await next();
});

// Configuration des endpoints
app.MapControllers();

// Endpoint de santé global
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

app.Run();
