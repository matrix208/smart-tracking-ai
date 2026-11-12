using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tracking.Core.Services;
using Tracking.Core.Workers;
using Tracking.Network;
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
    private readonly ILogger<TrackingRuntimeHostedService> _logger;

    private readonly PositionChannel _positionChannel;
    private readonly DeviceChannel _deviceChannel;
    private readonly AlarmChannel _alarmChannel;
    private readonly DeviceStateService _deviceStateService;
    private readonly DeviceRegistry _deviceRegistry;
    private readonly ProtocolPluginManager _pluginManager;

    private readonly PluginLifecycleManager _pluginLifecycleManager;

    // Fallback used only if no enabled plugin declares a DefaultPort
    // (should not normally happen, kept for safety).
    private const int FallbackPort = 5001;

    public TrackingRuntimeHostedService(
        ILogger<TrackingRuntimeHostedService> logger,
        PositionChannel positionChannel,
        DeviceChannel deviceChannel,
        AlarmChannel alarmChannel,
        DeviceStateService deviceStateService,
        DeviceRegistry deviceRegistry,
        ProtocolPluginManager pluginManager,
        PluginLifecycleManager pluginLifecycleManager)
    {
        _logger = logger;

        _positionChannel = positionChannel;
        _deviceChannel = deviceChannel;
        _alarmChannel = alarmChannel;
        _deviceStateService = deviceStateService;
        _deviceRegistry = deviceRegistry;
        _pluginManager = pluginManager;
        _pluginLifecycleManager = pluginLifecycleManager;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Tracking Runtime Started");

        // =====================================================
        // Load signed installed plugins through lifecycle manager
        // =====================================================

        try
        {
            var loadedPlugins =
                await _pluginLifecycleManager.LoadAllEnabledAsync(
                    stoppingToken);

            _logger.LogInformation(
                "Enabled installed plugins loaded: {Count}",
                loadedPlugins.Count);

            foreach (var plugin in loadedPlugins)
            {
                _logger.LogInformation(
                    "Plugin: {Id} | {Name} | Version={Version}",
                    plugin.PluginId,
                    plugin.Name,
                    plugin.Version);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load signed installed plugins.");

            throw;
        }

        // =====================================================
        // Packet Pipeline (shared across every listening port —
        // protocol detection is content-based via CanHandle, not
        // port-based, so any plugin can technically be reached on
        // any open port; separate ports simply match each device
        // family's factory/default server-port configuration).
        // =====================================================

        var pipeline =
            new PacketPipeline(
                _pluginManager);

        _logger.LogInformation(
            "Packet Pipeline Created with {PluginCount} plugins",
            _pluginManager.Plugins.Count);

        // =====================================================
        // Device Manager
        // =====================================================

        var deviceManagerLogger =
            LoggerFactory
                .Create(builder =>
                    builder.AddConsole())
                .CreateLogger<DeviceManager>();

        var deviceManager =
            new DeviceManager(
                deviceManagerLogger,
                _deviceRegistry,
                _positionChannel,
                _deviceChannel,
                _alarmChannel);

        // =====================================================
        // Shared event handlers (attached to every TCP listener)
        // =====================================================

        async Task OnClientDisconnectedAsync(ClientSession session)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(session.DeviceId))
                    return;

                if (!_deviceRegistry.TryGet(
                        session.DeviceId,
                        out var device) ||
                    device == null)
                    return;

                if (!ReferenceEquals(
                        device.Session,
                        session))
                    return;

                var imei = session.DeviceId;
                var protocol =
                    session.ProtocolId ??
                    "GT06";

                var lastSeen =
                    device.LastSeen;

                if (!_deviceRegistry.DisconnectSession(
                        session))
                    return;

                await _deviceChannel.WriteAsync(
                    new DeviceInfo
                    {
                        Imei = imei,
                        Protocol = protocol,
                        IsOnline = false,
                        LastSeen = lastSeen
                    });

                await _deviceStateService.UpdateOfflineAsync(
                    imei,
                    DateTime.UtcNow,
                    stoppingToken);

                _logger.LogInformation(
                    "Device disconnected. IMEI: {Imei}",
                    imei);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while processing client disconnect.");
            }
        }

        async Task OnPacketReceivedAsync(
            ClientSession session,
            ReadOnlyMemory<byte> packet)
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

                if (message.Type ==
                    MessageType.CommandResponse)
                {
                    await deviceManager.ProcessAsync(
                        session,
                        message);

                    return;
                }

                // =================================================
                // Save IMEI in Session after Login
                // =================================================

                if (message.Type ==
                        MessageType.Login &&
                    !string.IsNullOrWhiteSpace(
                        message.DeviceId))
                {
                    session.DeviceId =
                        message.DeviceId;
                }

                // =================================================
                // If message has no DeviceId,
                // use IMEI from Session
                // =================================================

                if (string.IsNullOrWhiteSpace(
                        message.DeviceId) &&
                    !string.IsNullOrWhiteSpace(
                        session.DeviceId))
                {
                    message =
                        message with
                        {
                            DeviceId =
                                session.DeviceId
                        };
                }

                // =================================================
                // Device Manager
                // =================================================

                await deviceManager.ProcessAsync(
                    session,
                    message);

            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while processing received packet.");
            }
        }

        // =====================================================
        // TCP Servers — one listener per distinct DefaultPort
        // among currently enabled plugins.
        // =====================================================

        var ports = _pluginManager.GetEnabledPlugins()
            .Select(p => p.Manifest.DefaultPort)
            .Where(port => port > 0)
            .Distinct()
            .ToList();

        if (ports.Count == 0)
        {
            _logger.LogWarning(
                "No enabled plugin declares a DefaultPort. Falling back to port {Port}.",
                FallbackPort);

            ports.Add(FallbackPort);
        }

        var listenerTasks = new List<Task>();

        foreach (var port in ports)
        {
            var server = new TcpTrackingServer(port);

            server.ClientDisconnected += OnClientDisconnectedAsync;
            server.PacketReceived += OnPacketReceivedAsync;

            _logger.LogInformation(
                "Starting TCP Tracking Server on port {Port}",
                port);

            listenerTasks.Add(server.StartAsync(stoppingToken));

            _logger.LogInformation(
                "TCP Tracking Server started on port {Port}",
                port);
        }

        // =====================================================
        // Keep Runtime Alive (each StartAsync already blocks
        // internally until cancellation; awaiting all of them
        // keeps this hosted service alive for as long as any
        // listener is running).
        // =====================================================

        try
        {
            await Task.WhenAll(listenerTasks);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }

        _logger.LogInformation(
            "Tracking Runtime Stopped");
    }

    private static string ResolvePluginRootPath(
        string path)
    {
        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path);
        }

        return Path.GetFullPath(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                path));
    }
}
