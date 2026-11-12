using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tracking.Core.Services;
using Tracking.Persistence.Channels;
using Tracking.Persistence.Services;
using Tracking.SDK.Models;

namespace Tracking.Core.Workers;

public sealed class HeartbeatMonitorWorker : BackgroundService
{
private static readonly TimeSpan CheckInterval =
TimeSpan.FromSeconds(30);

private static readonly TimeSpan OfflineTimeout =
    TimeSpan.FromMinutes(2);

private readonly ILogger<HeartbeatMonitorWorker> _logger;
private readonly DeviceRegistry _registry;
private readonly DeviceChannel _deviceChannel;
private readonly DeviceStateService _deviceStateService;

public HeartbeatMonitorWorker(
    ILogger<HeartbeatMonitorWorker> logger,
    DeviceRegistry registry,
    DeviceChannel deviceChannel,
    DeviceStateService deviceStateService)
{
    _logger = logger;
    _registry = registry;
    _deviceChannel = deviceChannel;
    _deviceStateService = deviceStateService;
}

protected override async Task ExecuteAsync(
    CancellationToken stoppingToken)
{
    _logger.LogInformation(
        "Heartbeat monitor started.");

    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            var now = DateTime.UtcNow;

            foreach (var device in _registry.Devices)
            {
                if (!device.IsOnline)
                    continue;

                if (now - device.LastSeen <= OfflineTimeout)
                    continue;

                try
                {
                    var protocol =
                        device.Session?.ProtocolId ?? "GT06";

                    _registry.Disconnect(device.Imei);

                    await _deviceChannel.WriteAsync(
                        new DeviceInfo
                        {
                            Imei = device.Imei,
                            Protocol = protocol,
                            IsOnline = false,
                            LastSeen = device.LastSeen
                        });

                    await _deviceStateService.UpdateOfflineAsync(
                        device.Imei,
                        now,
                        stoppingToken);

                    _logger.LogInformation(
                        "Device marked offline. IMEI: {Imei}",
                        device.Imei);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to process offline state for IMEI {Imei}",
                        device.Imei);
                }
            }

            await Task.Delay(
                CheckInterval,
                stoppingToken);
        }
        catch (OperationCanceledException)
        {
            break;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Heartbeat monitor loop failed.");

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }

    _logger.LogInformation(
        "Heartbeat monitor stopped.");
}

}
