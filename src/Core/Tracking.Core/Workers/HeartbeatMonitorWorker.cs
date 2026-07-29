using Microsoft.Extensions.Hosting;
using Tracking.Core.Services;
using Tracking.Persistence.Channels;
using Tracking.Storage.Entities;

namespace Tracking.Core.Workers;

public sealed class HeartbeatMonitorWorker : BackgroundService
{
    private readonly DeviceRegistry _registry;
    private readonly DeviceChannel _deviceChannel;


    private readonly TimeSpan _timeout =
        TimeSpan.FromMinutes(2);



    public HeartbeatMonitorWorker(
        DeviceRegistry registry,
        DeviceChannel deviceChannel)
    {
        _registry = registry;
        _deviceChannel = deviceChannel;
    }



    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        Console.WriteLine(
            "Heartbeat Monitor Started");


        while (!stoppingToken.IsCancellationRequested)
        {
            var now =
                DateTime.UtcNow;



            foreach (var device in _registry.Devices)
            {
                if (!device.Online)
                    continue;



                if (now - device.LastSeen > _timeout)
                {
                    _registry.Disconnect(
                        device.Imei);



                    await _deviceChannel.WriteAsync(
                        new DeviceEntity
                        {
                            Imei = device.Imei,
                            Protocol = "GT06",
                            Online = false,
                            LastSeen = now
                        });



                    Console.WriteLine(
                        $"[Heartbeat] Offline : {device.Imei}");
                }
            }



            await Task.Delay(
                TimeSpan.FromSeconds(30),
                stoppingToken);
        }
    }
}