using Microsoft.EntityFrameworkCore;
using Tracking.Storage.Entities;

namespace Tracking.Storage.Data;

public sealed class TrackingDbContext : DbContext
{
    public TrackingDbContext(
        DbContextOptions<TrackingDbContext> options)
        : base(options)
    {
    }


    public DbSet<DeviceEntity> Devices =>
        Set<DeviceEntity>();


    public DbSet<PositionEntity> Positions =>
        Set<PositionEntity>();


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceEntity>()
            .HasIndex(x => x.Imei)
            .IsUnique();


        modelBuilder.Entity<PositionEntity>()
            .HasOne(x => x.Device)
            .WithMany(x => x.Positions)
            .HasForeignKey(x => x.DeviceId);
    }
}
