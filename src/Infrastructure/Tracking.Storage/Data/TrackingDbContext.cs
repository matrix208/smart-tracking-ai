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

    public DbSet<CommandEntity> Commands =>
        Set<CommandEntity>();
    public DbSet<AlarmEntity> Alarms =>
        Set<AlarmEntity>();
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

        modelBuilder.Entity<CommandEntity>()
            .HasIndex(x => x.DeviceId);

        modelBuilder.Entity<CommandEntity>()
            .HasIndex(x => x.ServerFlag);

        modelBuilder.Entity<CommandEntity>()
            .Property(x => x.Command)
            .HasMaxLength(100);

        modelBuilder.Entity<CommandEntity>()
            .Property(x => x.Status)
            .HasMaxLength(30);

        modelBuilder.Entity<CommandEntity>()
            .Property(x => x.Protocol)
            .HasMaxLength(50);
        modelBuilder.Entity<AlarmEntity>()
        .HasOne(x => x.Device)
        .WithMany()
        .HasForeignKey(x => x.DeviceId);

    }
}