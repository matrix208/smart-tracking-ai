using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using Tracking.API.Auth;
using Tracking.API.Plugins;

using Tracking.Application.DependencyInjection;
using Tracking.Network;
using Tracking.PluginManager.Configuration;
using Tracking.Pipeline;
using Tracking.Runtime.DependencyInjection;
using Tracking.Security.Password;
using Tracking.Storage.Data;
using Tracking.Storage.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// MVC / API
// ============================================================

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ============================================================
// Storage
// ============================================================

builder.Services.AddTrackingStorage(
    builder.Configuration);

// ============================================================
// Application
// ============================================================

builder.Services.AddTrackingApplication();

// ============================================================
// Tracking Runtime
// ============================================================

builder.Services.AddTrackingRuntime(
    builder.Configuration);

// ============================================================
// JWT
// ============================================================

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(
        JwtOptions.SectionName));

builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddScoped<JwtTokenService>();

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey) ||
    jwtOptions.SigningKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT SigningKey must contain at least 32 characters.");
}

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtOptions.SigningKey)),

                ClockSkew = TimeSpan.FromSeconds(30)
            };
    });

// ============================================================
// Authorization
// ============================================================

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy =
        new Microsoft.AspNetCore.Authorization
            .AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});

// ============================================================
// Plugin Repository
// ============================================================
//
// Repository service is responsible for discovering installed
// plugin packages from the configured repository location.
//
// It is intentionally separated from the runtime plugin manager.
// ============================================================

builder.Services.Configure<PluginManagerOptions>(
    builder.Configuration.GetSection(
        PluginManagerOptions.SectionName));

builder.Services.AddSingleton<PluginRepositoryService>();

// ============================================================
// Build Application
// ============================================================

var app = builder.Build();

// ============================================================
// Database Initialization
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<TrackingDbContext>();

    db.Database.EnsureCreated();
}

// ============================================================
// Development Seed
// ============================================================

if (app.Environment.IsDevelopment())
{
    using var seedScope =
        app.Services.CreateScope();

    var seedDb = seedScope.ServiceProvider
        .GetRequiredService<TrackingDbContext>();

    if (!seedDb.Users.Any())
    {
        var passwordHasher =
            seedScope.ServiceProvider
                .GetRequiredService<PasswordHasher>();

        var passwordValidator =
            new PasswordValidator();

        const string username = "admin";
        const string password = "V9!qL2#xR7@pZ4";

        var validation =
            passwordValidator.Validate(
                password,
                username);

        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                string.Join(
                    Environment.NewLine,
                    validation.Errors));
        }

        seedDb.Users.Add(
            new Tracking.Storage.Entities.UserEntity
            {
                Username = username,
                PasswordHash =
                    passwordHasher.Hash(password),
                DisplayName =
                    "System Administrator",
                Role =
                    "Administrator",
                IsActive =
                    true,
                CreatedAt =
                    DateTime.UtcNow
            });

        seedDb.SaveChanges();

        Console.WriteLine(
            "Default administrator user created: admin");
    }
}

// ============================================================
// HTTP Pipeline
// ============================================================

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ============================================================
// OpenAPI
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ============================================================
// Run
// ============================================================

app.Run();
