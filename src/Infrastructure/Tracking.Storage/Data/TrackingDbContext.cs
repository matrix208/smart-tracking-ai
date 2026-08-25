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

    public DbSet<VehicleEntity> Vehicles =>
        Set<VehicleEntity>();

    public DbSet<DriverEntity> Drivers =>
        Set<DriverEntity>();

    public DbSet<DriverVehicleAssignmentEntity> DriverVehicleAssignments =>
        Set<DriverVehicleAssignmentEntity>();

    public DbSet<TripEntity> Trips =>
        Set<TripEntity>();

    public DbSet<TaskEntity> Tasks =>
        Set<TaskEntity>();


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VehicleEntity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.PlateNumber)
                .HasMaxLength(50);

            entity.Property(x => x.VehicleType)
                .HasMaxLength(100);

            entity.Property(x => x.Make)
                .HasMaxLength(100);

            entity.Property(x => x.Model)
                .HasMaxLength(100);

            entity.Property(x => x.Color)
                .HasMaxLength(50);

            entity.HasOne(x => x.Device)
                .WithMany()
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DriverEntity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(50);

            entity.Property(x => x.LicenseNumber)
                .HasMaxLength(100);

            entity.Property(x => x.EmployeeNumber)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<DriverVehicleAssignmentEntity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            entity.HasOne(x => x.Driver)
                .WithMany()
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Vehicle)
                .WithMany()
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.DriverId,
                x.StartAt
            });

            entity.HasIndex(x => new
            {
                x.VehicleId,
                x.StartAt
            });
        });


        modelBuilder.Entity<TripEntity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TripNumber)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => x.TripNumber)
                .IsUnique();

            entity.Property(x => x.Name)
                .HasMaxLength(200);

            entity.Property(x => x.Description)
                .HasMaxLength(1000);

            entity.Property(x => x.StartLocation)
                .HasMaxLength(500);

            entity.Property(x => x.EndLocation)
                .HasMaxLength(500);

            entity.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Notes)
                .HasMaxLength(1000);

            entity.HasOne(x => x.DriverVehicleAssignment)
                .WithMany()
                .HasForeignKey(x => x.DriverVehicleAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Driver)
                .WithMany()
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Vehicle)
                .WithMany()
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DriverId);
            entity.HasIndex(x => x.VehicleId);
            entity.HasIndex(x => x.DriverVehicleAssignmentId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.ScheduledStartAt);
        });

        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(1000);

            entity.Property(x => x.Type)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Address)
                .HasMaxLength(500);

            entity.Property(x => x.Notes)
                .HasMaxLength(1000);

            entity.HasOne(x => x.Trip)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new
            {
                x.TripId,
                x.Sequence
            });

            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.ScheduledAt);
        });

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
