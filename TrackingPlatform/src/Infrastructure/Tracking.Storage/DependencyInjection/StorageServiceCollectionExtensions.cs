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
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        // Used by scoped repositories such as DeviceRepository.
        // Options are Singleton so they can safely be shared with DbContextFactory.
        services.AddDbContext<TrackingDbContext>(
            options => options.UseSqlite(connectionString),
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Singleton);

        // Used by BackgroundServices / Singleton services.
        services.AddDbContextFactory<TrackingDbContext>(
            options => options.UseSqlite(connectionString));

        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IDeviceStateRepository, DeviceStateRepository>();

        return services;
    }
}