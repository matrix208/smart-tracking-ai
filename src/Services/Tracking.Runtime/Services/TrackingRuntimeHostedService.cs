using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tracking.Core.Services;
using Tracking.Core.Workers;
using Tracking.Network;
using Tracking.Network.Models;
using Tracking.Network.Servers;
using Tracking.Persistence.Channels;
using Tracking.Persistence.Services;
using Tracking.Pipeline;
using Tracking.PluginManager.Configuration;
using Tracking.PluginManager.Services;
using Tracking.PluginLoader.Services;
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

    private readonly PluginOptions _pluginOptions;
    private readonly PluginManagerOptions _pluginManagerOptions;

    public TrackingRuntimeHostedService(
        ILogger<TrackingRuntimeHostedService> logger,
        PositionChannel positionChannel,
        DeviceChannel deviceChannel,
        AlarmChannel alarmChannel,
        DeviceStateService deviceStateService,
        DeviceRegistry deviceRegistry,
        ProtocolPluginManager pluginManager,
        IOptions<PluginOptions> pluginOptions,
        IOptions<PluginManagerOptions> pluginManagerOptions)
    {
        _logger = logger;

        _positionChannel = positionChannel;
        _deviceChannel = deviceChannel;
        _alarmChannel = alarmChannel;
        _deviceStateService = deviceStateService;
        _deviceRegistry = deviceRegistry;
        _pluginManager = pluginManager;

        _pluginOptions = pluginOptions.Value;
        _pluginManagerOptions = pluginManagerOptions.Value;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Tracking Runtime Started");

        // =====================================================
        // Resolve plugin paths
        // =====================================================

        var pluginPath =
            ResolvePluginRootPath(
                _pluginOptions.RootPath);

        var installedPluginPath =
            ResolvePluginRootPath(
                _pluginManagerOptions.InstalledPluginsPath);

        _logger.LogInformation(
            "Development plugin directory: {PluginPath}",
            pluginPath);

        _logger.LogInformation(
            "Installed plugin directory: {InstalledPluginPath}",
            installedPluginPath);

        _logger.LogInformation(
            "Development plugin directory exists: {Exists}",
            Directory.Exists(pluginPath));

        _logger.LogInformation(
            "Installed plugin directory exists: {Exists}",
            Directory.Exists(installedPluginPath));

        // =====================================================
        // Load plugins
        // =====================================================

        var loader =
            new Tracking.PluginLoader.Services.PluginLoader();

        try
        {
            IReadOnlyList<Tracking.SDK.Interfaces.IProtocolPlugin> plugins;

            if (_pluginManagerOptions.LoadEnabledOnly)
            {
                // =================================================
                // PRODUCTION PLUGIN LOADING
                //
                // InstalledPluginStore is the source of truth.
                //
                // Runtime loads ONLY:
                //   data/plugins/{pluginId}
                //
                // and ONLY when manifest.json has:
                //   "enabled": true
                //
                // The development plugins directory is NOT used
                // when LoadEnabledOnly=true.
                // =================================================

                var installedStore =
                    new InstalledPluginStore(
                        installedPluginPath);

                var installed =
                    installedStore.GetAll();

                _logger.LogInformation(
                    "Installed plugins found: {Count}",
                    installed.Count);

                foreach (var installedPlugin in installed)
                {
                    _logger.LogInformation(
                        "Installed Plugin: {Id} | {Name} | Version={Version} | Enabled={Enabled}",
                        installedPlugin.Id,
                        installedPlugin.Name,
                        installedPlugin.Version,
                        installedPlugin.Enabled);
                }

                var enabledPluginIds =
                    installed
                        .Where(x => x.Enabled)
                        .Select(x => x.Id)
                        .ToHashSet(
                            StringComparer.OrdinalIgnoreCase);

                _logger.LogInformation(
                    "Enabled installed plugins: {Count}",
                    enabledPluginIds.Count);

                // =================================================
                // IMPORTANT
                //
                // Load directly from the installed plugin store.
                //
                // Do NOT load from:
                //     plugins/
                //
                // The installed directory is the runtime source.
                // =================================================

                plugins =
                    await loader.LoadAsync(
                        installedPluginPath,
                        enabledPluginIds,
                        stoppingToken);
            }
            else
            {
                // =================================================
                // DEVELOPMENT MODE
                //
                // When LoadEnabledOnly=false, the development
                // plugins directory can be loaded normally.
                // =================================================

                plugins =
                    await loader.LoadAsync(
                        pluginPath,
                        stoppingToken);
            }

            _pluginManager.Register(plugins);

            _logger.LogInformation(
                "Plugins Loaded: {Count}",
                plugins.Count);

            foreach (var plugin in plugins)
            {
                _logger.LogInformation(
                    "Plugin: {Id} | {Name} | Version={Version}",
                    plugin.Manifest.Id,
                    plugin.Manifest.Name,
                    plugin.Manifest.Version);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load plugins from {PluginPath}",
                pluginPath);

            throw;
        }

        // =====================================================
        // Packet Pipeline
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
        // TCP Server
        // =====================================================

        var server =
            new TcpTrackingServer(
                5001);

        server.ClientDisconnected += async session =>
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
        };

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
                    session is ClientSession loginSession &&
                    !string.IsNullOrWhiteSpace(
                        message.DeviceId))
                {
                    loginSession.DeviceId =
                        message.DeviceId;
                }

                // =================================================
                // If message has no DeviceId,
                // use IMEI from Session
                // =================================================

                if (string.IsNullOrWhiteSpace(
                        message.DeviceId) &&
                    session is ClientSession clientSession &&
                    !string.IsNullOrWhiteSpace(
                        clientSession.DeviceId))
                {
                    message =
                        message with
                        {
                            DeviceId =
                                clientSession.DeviceId
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
        };

        // =====================================================
        // Start TCP Server
        // =====================================================

        _logger.LogInformation(
            "Starting TCP Tracking Server on port 5001");

        await server.StartAsync(
            stoppingToken);

        _logger.LogInformation(
            "TCP Tracking Server started on port 5001");

        // =====================================================
        // Keep Runtime Alive
        // =====================================================

        try
        {
            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }

        // =====================================================
        // Stop TCP Server
        // =====================================================

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
