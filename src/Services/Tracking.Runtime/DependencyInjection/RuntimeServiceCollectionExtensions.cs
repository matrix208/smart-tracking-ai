using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Tracking.Core.Services;
using Tracking.Core.Workers;

using Tracking.Persistence.Channels;
using Tracking.Persistence.Services;
using Tracking.Persistence.Workers;

using Tracking.PluginManager.Configuration;
using Tracking.PluginManager.Services;

namespace Tracking.Runtime.DependencyInjection;

public static class RuntimeServiceCollectionExtensions
{
    public static IServiceCollection AddTrackingRuntime(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // =====================================================
        // Plugin Configuration
        // =====================================================

        services.Configure<PluginOptions>(
            configuration.GetSection(
                PluginOptions.SectionName));

        // =====================================================
        // Plugin Runtime
        // =====================================================

        services.AddSingleton<ProtocolPluginManager>();

        services.AddSingleton<Tracking.PluginManager.Services.PluginLifecycleManager>();

        services.Configure<PluginManagerOptions>(
            configuration.GetSection(
                PluginManagerOptions.SectionName));

        services.AddSingleton<InstalledPluginStore>(sp =>
        {
            var options =
                sp.GetRequiredService<IOptions<PluginManagerOptions>>()
                    .Value;

            var path = options.InstalledPluginsPath;

            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        path));
            }

            return new InstalledPluginStore(path);
        });

        services.AddSingleton<Tracking.Plugin.Abstractions.Interfaces.IPluginInstaller>(sp =>
        {
            var installedStore = sp.GetRequiredService<InstalledPluginStore>();

            var publicKeyPath = configuration["PackageSecurity:PublicKeyPath"]
                ?? throw new InvalidOperationException(
                    "PackageSecurity:PublicKeyPath is not configured.");

            if (!Path.IsPathRooted(publicKeyPath))
            {
                publicKeyPath = Path.GetFullPath(
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        publicKeyPath));
            }

            var requireSignature = configuration.GetValue<bool>(
                "PackageSecurity:RequireSignature",
                defaultValue: true);

            return new Tracking.Plugin.Runtime.Installer.PluginInstaller(
                installedStore,
                publicKeyPath,
                requireSignature);
        });

        // =====================================================
        // Core Services
        // =====================================================

        services.AddSingleton<DeviceRegistry>();
        services.AddSingleton<DeviceStateService>();

        // =====================================================
        // Data Channels
        // =====================================================

        services.AddSingleton<PositionChannel>();
        services.AddSingleton<DeviceChannel>();
        services.AddSingleton<AlarmChannel>();

        // =====================================================
        // Persistence Workers
        // =====================================================

        services.AddHostedService<PositionWriterWorker>();
        services.AddHostedService<DeviceWriterWorker>();
        services.AddHostedService<AlarmWriterWorker>();

        // =====================================================
        // Device State Monitor
        // =====================================================

        services.AddHostedService<HeartbeatMonitorWorker>();

        // =====================================================
        // Runtime Engine
        // =====================================================

        services.AddHostedService<
            Services.TrackingRuntimeHostedService>();

        return services;
    }
}
