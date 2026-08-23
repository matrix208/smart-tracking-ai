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

    public DbSet<UserEntity> Users =>
        Set<UserEntity>();

        public DbSet<DeviceModelEntity> DeviceModels => Set<DeviceModelEntity>();

public DbSet<PeripheralTypeEntity> PeripheralTypes => Set<PeripheralTypeEntity>();

public DbSet<DevicePeripheralEntity> DevicePeripherals => Set<DevicePeripheralEntity>();

public DbSet<DeviceStateEntity> DeviceStates => Set<DeviceStateEntity>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceEntity>()
            .HasIndex(x => x.Imei)
            .IsUnique();

        modelBuilder.Entity<DeviceEntity>()
            .HasAlternateKey(x => x.Imei);


        modelBuilder.Entity<PositionEntity>()
            .HasOne(x => x.Device)
            .WithMany(x => x.Positions)
            .HasForeignKey(x => x.DeviceId)
            .HasPrincipalKey(x => x.Imei);


        modelBuilder.Entity<AlarmEntity>()
            .HasOne(x => x.Device)
            .WithMany()
            .HasForeignKey(x => x.DeviceId)
            .HasPrincipalKey(x => x.Imei);


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
            modelBuilder.Entity<DeviceEntity>()
    .HasOne(d => d.DeviceModel)
    .WithMany(m => m.Devices)
    .HasForeignKey(d => d.DeviceModelId)
    .OnDelete(DeleteBehavior.SetNull);

modelBuilder.Entity<DevicePeripheralEntity>()
    .HasOne(p => p.Device)
    .WithMany(d => d.Peripherals)
    .HasForeignKey(p => p.DeviceId);

modelBuilder.Entity<DevicePeripheralEntity>()
    .HasOne(p => p.PeripheralType)
    .WithMany(t => t.Peripherals)
    .HasForeignKey(p => p.PeripheralTypeId);
    
    modelBuilder.Entity<DeviceStateEntity>(entity =>
{
    entity.HasKey(x => x.DeviceId);

    entity.Property(x => x.DeviceId)
        .HasMaxLength(50);

    entity.Property(x => x.LastUpdate);

    entity.Property(x => x.Latitude);

    entity.Property(x => x.Longitude);

    entity.Property(x => x.Speed);

    entity.Property(x => x.Course);

    entity.Property(x => x.Online);

    entity.Property(x => x.Ignition);

    entity.Property(x => x.Satellites);

    entity.Property(x => x.Battery);

    entity.Property(x => x.Signal);
});
    }
}
