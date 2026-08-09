using Microsoft.Extensions.DependencyInjection;
using Tracking.Core.Services;
using Tracking.Core.Workers;
using Tracking.Persistence.Channels;
using Tracking.Persistence.Services;
using Tracking.Persistence.Workers;

namespace Tracking.Runtime.DependencyInjection;

public static class RuntimeServiceCollectionExtensions
{
public static IServiceCollection AddTrackingRuntime(
this IServiceCollection services)
{
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
