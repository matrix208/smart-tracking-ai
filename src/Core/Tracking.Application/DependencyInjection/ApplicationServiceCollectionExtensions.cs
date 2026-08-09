using Microsoft.Extensions.DependencyInjection;
using Tracking.Application.Interfaces;
using Tracking.Application.Services;

namespace Tracking.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddTrackingApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IPositionService, PositionService>();

        services.AddScoped<IDeviceService, DeviceService>();

        services.AddScoped<IDeviceStateService, DeviceStateService>();

        return services;
    }
}