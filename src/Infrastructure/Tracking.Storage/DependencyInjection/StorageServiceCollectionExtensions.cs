using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tracking.Storage.Data;
using Tracking.Storage.Repositories;

namespace Tracking.Storage.DependencyInjection;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddTrackingStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TrackingDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IDeviceRepository, DeviceRepository>();

        // سنضيفها لاحقاً
        // services.AddScoped<IPositionRepository, PositionRepository>();
        // services.AddScoped<IAlarmRepository, AlarmRepository>();

        return services;
    }
}