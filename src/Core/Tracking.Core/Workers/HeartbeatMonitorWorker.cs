using Microsoft.Extensions.Hosting;
using Tracking.Core.Services;
using Tracking.Persistence.Channels;
using Tracking.SDK.Models;

namespace Tracking.Core.Workers;

public sealed class HeartbeatMonitorWorker : BackgroundService
{
    private static readonly TimeSpan CheckInterval =
        TimeSpan.FromSeconds(30);

    private static readonly TimeSpan OfflineTimeout =
        TimeSpan.FromMinutes(2);

    private readonly DeviceRegistry _registry;
    private readonly DeviceChannel _deviceChannel;

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
        Console.WriteLine("Heartbeat Monitor Started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            foreach (var device in _registry.Devices)
            {
                if (!device.IsOnline)
                    continue;

                if (now - device.LastSeen <= OfflineTimeout)
                    continue;

                // احتفظ بالبروتوكول قبل فصل الجلسة
                var protocol =
                    device.Session?.ProtocolId ?? "GT06";

                _registry.Disconnect(device.Imei);

                await _deviceChannel.WriteAsync(
                    new DeviceInfo
                    {
                        Imei = device.Imei,
                        Protocol = protocol,
                        IsOnline = false,
                        LastSeen = now
                    });

                Console.WriteLine(
                    $"[Heartbeat] Device Offline : {device.Imei}");
            }

            try
            {
                await Task.Delay(
                    CheckInterval,
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}