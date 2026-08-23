using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tracking.Core.Services;
using Tracking.Network.Models;
using Tracking.Network.Servers;
using Tracking.Persistence.Channels;
using Tracking.Persistence.Services;
using Tracking.Pipeline;
using Tracking.PluginManager.Services;
using Tracking.SDK.Enums;
using Tracking.SDK.Models;

namespace Tracking.Runtime.Services;

public sealed class TrackingRuntimeHostedService : BackgroundService
{
    private readonly ILogger _logger;

    private readonly PositionChannel _positionChannel;
    private readonly DeviceChannel _deviceChannel;
    private readonly AlarmChannel _alarmChannel;
    private readonly DeviceStateService _deviceStateService;

    public TrackingRuntimeHostedService(
        ILogger<TrackingRuntimeHostedService> logger,
        PositionChannel positionChannel,
        DeviceChannel deviceChannel,
        AlarmChannel alarmChannel,
        DeviceStateService deviceStateService)
    {
        _logger = logger;
        _positionChannel = positionChannel;
        _deviceChannel = deviceChannel;
        _alarmChannel = alarmChannel;
        _deviceStateService = deviceStateService;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Tracking Runtime Started");

        // =====================================================
        // Load Plugins
        // =====================================================

        var loader =
            new Tracking.PluginLoader.Services.PluginLoader();

        var basePath =
            Directory.GetCurrentDirectory();

        var pluginPath =
            Path.GetFullPath(
                Path.Combine(
                    basePath,
                    "../../Plugins"));

        var plugins =
            await loader.LoadAsync(
                pluginPath,
                stoppingToken);

        var pluginManager =
            new ProtocolPluginManager();

        pluginManager.Register(
            plugins);

        _logger.LogInformation(
            "Plugins Loaded: {Count}",
            plugins.Count);

        foreach (var plugin in plugins)
        {
            _logger.LogInformation(
                "Plugin: {Name}",
                plugin.Manifest.Name);
        }

        // =====================================================
        // Packet Pipeline
        // =====================================================

        var pipeline =
            new PacketPipeline(
                plugins);

        _logger.LogInformation(
            "Packet Pipeline Created");

        // =====================================================
        // Device Manager
        // =====================================================

        var deviceRegistry =
            new DeviceRegistry();

        var deviceManagerLogger =
            LoggerFactory
                .Create(builder => builder.AddConsole())
                .CreateLogger<DeviceManager>();

        var deviceManager =
            new DeviceManager(
                deviceManagerLogger,
                deviceRegistry,
                _positionChannel,
                _deviceChannel,
                _alarmChannel);

        // =====================================================
        // TCP Server
        // =====================================================

        var server =
            new TcpTrackingServer(
                5001);

        server.PacketReceived += async (
            session,
            packet) =>
        {
            try
            {
                _logger.LogInformation(
                    "Packet Received Length={Length}",
                    packet.Length);

                var message =
                    await pipeline.ProcessAsync(
                        packet,
                        session);

                if (message == null)
                    return;

                // =================================================
                // Command Response
                // =================================================

                if (message.Type == MessageType.CommandResponse)
                {
                    await deviceManager.ProcessAsync(
                        session,
                        message);

                    return;
                }

                // =================================================
                // حفظ IMEI في Session بعد Login
                // =================================================

                if (message.Type == MessageType.Login &&
                    session is ClientSession loginSession &&
                    !string.IsNullOrWhiteSpace(message.DeviceId))
                {
                    loginSession.DeviceId =
                        message.DeviceId;
                }

                // =================================================
                // إذا وصلت الرسالة بدون DeviceId
                // نأخذ IMEI من Session
                // =================================================

                if (string.IsNullOrWhiteSpace(message.DeviceId) &&
                    session is ClientSession clientSession &&
                    !string.IsNullOrWhiteSpace(clientSession.DeviceId))
                {
                    message =
                        message with
                        {
                            DeviceId =
                                clientSession.DeviceId,

                            Alarm =
                                message.Alarm == null
                                    ? null
                                    : new Alarm
                                    {
                                        DeviceId =
                                            clientSession.DeviceId,

                                        AlarmCode =
                                            message.Alarm.AlarmCode,

                                        DeviceTime =
                                            message.Alarm.DeviceTime,

                                        ServerTime =
                                            message.Alarm.ServerTime
                                    }
                        };
                }

                await deviceManager.ProcessAsync(
                    session,
                    message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Packet processing failed.");
            }
        };

        // =====================================================
        // Client Disconnected
        // =====================================================

        server.ClientDisconnected += async session =>
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(session.DeviceId))
                {
                    var deviceId =
                        session.DeviceId;

                    var offlineTime =
                        DateTime.UtcNow;

                    // ---------------------------------------------
                    // Registry
                    // ---------------------------------------------

                    var connectedDevice =
                        deviceRegistry.Devices
                            .FirstOrDefault(d => d.Imei == deviceId);

                    var lastSeen =
                        connectedDevice?.LastSeen ?? offlineTime;

                    deviceRegistry.Disconnect(
                        deviceId);

                    // ---------------------------------------------
                    // DeviceStates
                    // ---------------------------------------------

                    await _deviceStateService.UpdateOfflineAsync(
                        deviceId,
                        offlineTime);

                    // ---------------------------------------------
                    // Devices
                    // ---------------------------------------------

                    await _deviceChannel.WriteAsync(
                        new DeviceInfo
                        {
                            Imei = deviceId,
                            Protocol =
                                session.ProtocolId,
                            IsOnline = false,
                            LastSeen =
                                lastSeen
                        });

                    _logger.LogInformation(
                        "Device Offline: {Imei}",
                        deviceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process disconnected device.");
            }
        };

        // =====================================================
        // Start TCP Server
        // =====================================================

        _logger.LogInformation(
            "TCP Server Starting : 5001");

        await server.StartAsync(
            stoppingToken);
    }
}
