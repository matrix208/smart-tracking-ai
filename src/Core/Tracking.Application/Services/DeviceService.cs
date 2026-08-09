using Tracking.Application.DTOs;
using Tracking.Application.Interfaces;
using Tracking.Storage.Repositories;

namespace Tracking.Application.Services;

public sealed class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _repository;

    public DeviceService(
        IDeviceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<DeviceDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var devices = await _repository.GetAllAsync(
            cancellationToken);

        return devices
            .Select(Map)
            .ToList();
    }

    public async Task<DeviceDto?> GetDetailsAsync(
        string imei,
        CancellationToken cancellationToken = default)
    {
        var device = await _repository.GetDetailsAsync(
            imei,
            cancellationToken);

        return device == null
            ? null
            : Map(device);
    }

   private static DeviceDto Map(
    Tracking.Storage.Entities.DeviceEntity device)
{
    return new DeviceDto
    {
        Imei = device.Imei,
        Protocol = device.Protocol,

        IsOnline = device.IsOnline,
        LastSeen = device.LastSeen,

        LastLatitude = device.LastLatitude,
        LastLongitude = device.LastLongitude,
        LastSpeed = device.LastSpeed,
        LastCourse = device.LastCourse,

            Model = device.DeviceModel == null
            ? null
            : new DeviceModelDto
            {
                Name = device.DeviceModel.Model,
                Manufacturer = device.DeviceModel.Manufacturer
            },

        Peripherals = device.Peripherals
            .Select(x => new PeripheralDto
            {
                Type = x.PeripheralType != null
                    ? x.PeripheralType.Name
                    : string.Empty,

                Name = x.Name,

                Enabled = x.Enabled
            })
            .ToList()
    };
}
}