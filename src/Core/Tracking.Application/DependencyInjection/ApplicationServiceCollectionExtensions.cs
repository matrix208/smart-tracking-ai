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

            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IDriverService, DriverService>();
            services.AddScoped<IDriverVehicleAssignmentService, DriverVehicleAssignmentService>();
            services.AddScoped<ITripService, TripService>();
        services.AddScoped<ITaskService, TaskService>();

        return services;
    }
}