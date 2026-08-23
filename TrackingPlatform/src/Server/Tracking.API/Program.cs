using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using Tracking.API.Auth;
using Tracking.Application.DependencyInjection;
using Tracking.Network;
using Tracking.Pipeline;
using Tracking.Runtime.DependencyInjection;
using Tracking.Security.Password;
using Tracking.Storage.Data;
using Tracking.Storage.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddTrackingStorage(
    builder.Configuration);

builder.Services.AddTrackingApplication();
builder.Services.AddTrackingRuntime();

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

var signingKey =
    builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException(
        "JWT SigningKey is missing. Set Jwt__SigningKey as an environment variable.");

if (signingKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT SigningKey must contain at least 32 characters.");
}

jwtOptions.SigningKey = signingKey;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                            signingKey)),

                ClockSkew = TimeSpan.FromSeconds(30)
            };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy =
        new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<TrackingDbContext>();

    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    using var seedScope = app.Services.CreateScope();

    var seedDb = seedScope.ServiceProvider
        .GetRequiredService<TrackingDbContext>();

    if (!seedDb.Users.Any())
    {
        var passwordHasher = seedScope.ServiceProvider
            .GetRequiredService<PasswordHasher>();

        var passwordValidator = new PasswordValidator();

        var username =
            builder.Configuration["Admin:Username"]
            ?? "admin";

        var password =
            builder.Configuration["Admin:Password"]
            ?? throw new InvalidOperationException(
                "Admin password is missing. Set Admin__Password as an environment variable.");

        var validation = passwordValidator.Validate(
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
                PasswordHash = passwordHasher.Hash(password),
                DisplayName = "System Administrator",
                Role = "Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

        seedDb.SaveChanges();

        Console.WriteLine(
            $"Default administrator user created: {username}");
    }

    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
