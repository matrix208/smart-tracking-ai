using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using Tracking.API.Auth;
using Tracking.API.Plugins;
using Tracking.API.Settings;

using Microsoft.Extensions.FileProviders;
using System.IO;

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

    // One "permission.<key>" policy per seeded permission key, so
    // controllers can use [Authorize(Policy = "permission.roles.manage")]
    // instead of role-name checks where fine-grained control is needed.
    string[] permissionKeys =
    [
        "dashboard.view", "livemap.view",
        "devices.view", "devices.manage",
        "vehicles.view", "vehicles.manage",
        "drivers.view", "drivers.manage",
        "trips.view", "trips.manage",
        "tasks.view", "tasks.manage",
        "reports.view", "reports.export",
        "plugins.view", "plugins.manage",
        "settings.view", "settings.manage",
        "users.view", "users.manage",
        "roles.view", "roles.manage",
        "ai.view"
    ];

    foreach (var key in permissionKeys)
    {
        options.AddPolicy($"permission.{key}", policy =>
            policy.Requirements.Add(
                new Tracking.API.Auth.PermissionRequirement(key)));
    }
});

builder.Services.AddSingleton<
    Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    Tracking.API.Auth.PermissionAuthorizationHandler>();

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

builder.Services.AddSingleton<ILoginMediaSettingsService, LoginMediaSettingsService>();

// ============================================================
// Build Application
// ============================================================

var app = builder.Build();

// ============================================================
// Static Files (uploaded login media)
// ============================================================

var uploadsRoot = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(Path.Combine(uploadsRoot, "login-media"));

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/uploads"
});

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

    SeedRolesAndPermissions(seedDb);
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


// ============================================================
// RBAC Seed (Roles / Permissions)
//
// Runs on every development startup, independent of whether the
// Users table already had rows. Safe to re-run: each section is
// gated by its own "if not any" check.
// ============================================================

static void SeedRolesAndPermissions(Tracking.Storage.Data.TrackingDbContext db)
{
    var permissionSeed = new (string Key, string Name, string Category)[]
    {
        ("dashboard.view", "View Dashboard", "Dashboard"),
        ("livemap.view", "View Live Map", "Live Map"),
        ("devices.view", "View Devices", "Devices"),
        ("devices.manage", "Manage Devices", "Devices"),
        ("vehicles.view", "View Vehicles", "Vehicles"),
        ("vehicles.manage", "Manage Vehicles", "Vehicles"),
        ("drivers.view", "View Drivers", "Drivers"),
        ("drivers.manage", "Manage Drivers", "Drivers"),
        ("trips.view", "View Trips", "Trips"),
        ("trips.manage", "Manage Trips", "Trips"),
        ("tasks.view", "View Tasks", "Tasks"),
        ("tasks.manage", "Manage Tasks", "Tasks"),
        ("reports.view", "View Reports", "Reports"),
        ("reports.export", "Export Reports", "Reports"),
        ("plugins.view", "View Plugins", "Plugins"),
        ("plugins.manage", "Manage Plugins", "Plugins"),
        ("settings.view", "View Settings", "Settings"),
        ("settings.manage", "Manage Settings", "Settings"),
        ("users.view", "View Users", "Users"),
        ("users.manage", "Manage Users", "Users"),
        ("roles.view", "View Roles", "Roles & Permissions"),
        ("roles.manage", "Manage Roles", "Roles & Permissions"),
        ("ai.view", "View AI Insights", "AI Insights"),
    };

    if (!db.Permissions.Any())
    {
        foreach (var (key, name, category) in permissionSeed)
        {
            db.Permissions.Add(new Tracking.Storage.Entities.PermissionEntity
            {
                Key = key,
                Name = name,
                Description = name,
                Category = category
            });
        }

        db.SaveChanges();

        Console.WriteLine("Default permissions seeded.");
    }

    if (!db.Roles.Any())
    {
        var allPermissions = db.Permissions.ToList();

        var administrator = new Tracking.Storage.Entities.RoleEntity
        {
            Name = "Administrator",
            Description = "Full access to every feature.",
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow
        };

        var manager = new Tracking.Storage.Entities.RoleEntity
        {
            Name = "Manager",
            Description = "Operational access without user, role or settings management.",
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow
        };

        var viewer = new Tracking.Storage.Entities.RoleEntity
        {
            Name = "Viewer",
            Description = "Read-only access across the platform.",
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Roles.AddRange(administrator, manager, viewer);
        db.SaveChanges();

        var managerExcludedKeys = new HashSet<string>
        {
            "users.manage",
            "roles.manage",
            "roles.view",
            "settings.manage"
        };

        foreach (var permission in allPermissions)
        {
            db.RolePermissions.Add(new Tracking.Storage.Entities.RolePermissionEntity
            {
                RoleId = administrator.Id,
                PermissionId = permission.Id
            });

            if (!managerExcludedKeys.Contains(permission.Key))
            {
                db.RolePermissions.Add(new Tracking.Storage.Entities.RolePermissionEntity
                {
                    RoleId = manager.Id,
                    PermissionId = permission.Id
                });
            }

            if (permission.Key.EndsWith(".view"))
            {
                db.RolePermissions.Add(new Tracking.Storage.Entities.RolePermissionEntity
                {
                    RoleId = viewer.Id,
                    PermissionId = permission.Id
                });
            }
        }

        db.SaveChanges();

        Console.WriteLine("Default roles (Administrator, Manager, Viewer) seeded.");
    }

    // Ensure every user still carrying the legacy Role = "Administrator"
    // string has an explicit UserRoles link to the Administrator role,
    // even on a database that already existed before this feature shipped.
    var administratorRole = db.Roles.FirstOrDefault(r => r.Name == "Administrator");

    if (administratorRole is not null)
    {
        var adminUsers = db.Users
            .Where(u => u.Role == "Administrator")
            .ToList();

        var linkedAny = false;

        foreach (var user in adminUsers)
        {
            var hasLink = db.UserRoles
                .Any(ur => ur.UserId == user.Id && ur.RoleId == administratorRole.Id);

            if (!hasLink)
            {
                db.UserRoles.Add(new Tracking.Storage.Entities.UserRoleEntity
                {
                    UserId = user.Id,
                    RoleId = administratorRole.Id
                });

                linkedAny = true;
            }
        }

        if (linkedAny)
        {
            db.SaveChanges();

            Console.WriteLine("Linked existing Administrator user(s) to the Administrator role.");
        }
    }
}
