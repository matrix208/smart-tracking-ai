using Tracking.Core.Services;
using Tracking.Network.Models;
using Tracking.Network.Servers;
using Tracking.Persistence.Channels;
using Tracking.Persistence.Workers;
using Tracking.Pipeline;
using Tracking.PluginLoader.Services;
using Tracking.SDK.Enums;
using Tracking.Storage.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tracking.Core.Workers;
using Tracking.PluginManager.Services;
using Tracking.Commands.Channels;
using Tracking.Commands.Services;
using Tracking.Commands.Workers;
using Tracking.Commands.Stores;
using Tracking.SDK.Models;
using Tracking.Commands.Lifecycle;


var loader = new PluginLoader();

var pluginManager = new ProtocolPluginManager();

var plugins =
    await loader.LoadAsync(
        "src/Plugins");

pluginManager.Register(
    plugins);


Console.WriteLine("Plugins Loaded");
Console.WriteLine("----------------");


foreach (var plugin in plugins)
{
    Console.WriteLine(
        plugin.Manifest.Name);
}


Console.WriteLine();


var pipeline = new PacketPipeline(
    plugins);


var registry = new DeviceRegistry();
var commandChannel = new CommandChannel();

var commandService =
    new CommandService(commandChannel);

var commandDispatcher =
    new CommandDispatcher(commandService);
    var commandSequence =
    new Tracking.Commands.Sequence.CommandSequence();
    var pendingStore =
    new PendingCommandStore();

    var commandTimeoutWorker =
    new CommandTimeoutWorker(
        pendingStore);

var positionChannel = new PositionChannel();

var deviceChannel = new DeviceChannel();



// Database
var options =
    new DbContextOptionsBuilder<TrackingDbContext>()
        .UseSqlite(
            "Data Source=tracking.db")
        .Options;

var factory =
    new PooledDbContextFactory<TrackingDbContext>(
        options);

// إنشاء قاعدة البيانات والجداول إذا لم تكن موجودة
await using (var db = factory.CreateDbContext())
{
    await db.Database.EnsureCreatedAsync();
}

await using var commandDb =
    factory.CreateDbContext();

var commandLifecycle =
    new CommandLifecycleService(
        pendingStore,
        commandDb);

await using var historyDb =
    factory.CreateDbContext();

var historyService =
    new Tracking.Commands.Queries.CommandHistoryService(
        historyDb);

var commandWorker =
    new CommandWorker(
        commandChannel,
        registry,
        pluginManager,
        commandSequence,
        pendingStore,
        commandLifecycle);
// تشغيل حفظ المواقع
var positionWriter =
    new PositionWriterWorker(
        positionChannel,
        factory);

var workerCts =
    new CancellationTokenSource();


_ = positionWriter.StartAsync(
    workerCts.Token);

Console.WriteLine(
    "Position Writer Started");

// تشغيل حفظ الأجهزة
var deviceWriter =
    new DeviceWriterWorker(
        deviceChannel,
        factory);

_ = deviceWriter.StartAsync(
    workerCts.Token);

Console.WriteLine(
    "Device Writer Started");

// مراقبة Heartbeat
var heartbeatWorker =
    new HeartbeatMonitorWorker(
        registry,
        deviceChannel);

_ = heartbeatWorker.StartAsync(
    workerCts.Token);


Console.WriteLine(
    "Heartbeat Monitor Started");

_ = commandWorker.StartAsync(
    workerCts.Token);
    
_ = commandTimeoutWorker.StartAsync(
    workerCts.Token);

Console.WriteLine(
    "Command Worker Started");

// Device Manager
var deviceManager =
    new DeviceManager(
        registry,
        positionChannel,
        deviceChannel);

// TCP Server
var server =
    new TcpTrackingServer(
        5001);


server.PacketReceived += async (
    session,
    packet) =>
{
    var message =
        await pipeline.ProcessAsync(
            packet,
            session);

if (message == null)
    return;

// معالجة ردود الأوامر القادمة من أي Plugin
if (message.Type == MessageType.CommandResponse &&
    message.Payload is CommandResult result)
{
    if (pendingStore.TryGet(
            result.ServerFlag,
            out var command))
    {
        await commandLifecycle.CompleteAsync(result);

        Console.WriteLine(
            $"[Command] Reply -> {command!.DeviceId}");

        Console.WriteLine(
            $"Success : {result.Success}");

        Console.WriteLine(
            $"Response: {result.Response}");
    }
    else
    {
        Console.WriteLine(
            $"[Command] Unknown ServerFlag : {result.ServerFlag}");
    }

    return;
}

    // إذا وصلت GPS بدون IMEI
    // نأخذ IMEI من Session
    if (message.Type == MessageType.Position &&
        string.IsNullOrWhiteSpace(message.DeviceId) &&
        session is ClientSession clientSession &&
        !string.IsNullOrWhiteSpace(clientSession.DeviceId))
    {
        message =
            message with
            {
                DeviceId = clientSession.DeviceId
            };
    }

    // حفظ IMEI بعد Login
    if (message.Type == MessageType.Login &&
        session is ClientSession loginSession &&
        !string.IsNullOrWhiteSpace(message.DeviceId))
    {
        loginSession.DeviceId =
            message.DeviceId;
    }

    await deviceManager.ProcessAsync(
        session,
        message);

    Console.WriteLine(
        $"Decoded: {message.Type}");

    PrintRegistry(
        registry);
};

// عند فصل الجهاز
server.ClientDisconnected += async session =>
{
    if (!string.IsNullOrWhiteSpace(session.DeviceId))
    {
        registry.Disconnect(
            session.DeviceId);

        await deviceChannel.WriteAsync(
            new Tracking.Storage.Entities.DeviceEntity
            {
                Imei = session.DeviceId,
                Protocol = "GT06",
                Online = false,
                LastSeen = DateTime.UtcNow
            });


        Console.WriteLine(
            $"[Registry] Device Offline : {session.DeviceId}");
    }

    return;

};

_ = server.StartAsync(workerCts.Token);

Console.WriteLine("Server Started");

while (true)
{
    Console.Write("> ");

    var line = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(line))
        continue;

    var parts = line.Split(
        ' ',
        StringSplitOptions.RemoveEmptyEntries);

    if (parts.Length < 2)
    {
        Console.WriteLine(
    "Usage: position|status|reboot|history <imei>");
        continue;
    }

    switch (parts[0].ToLowerInvariant())
    {
        case "position":
            await commandDispatcher.RequestPositionAsync(parts[1]);
            break;

        case "status":
            await commandDispatcher.RequestStatusAsync(parts[1]);
            break;

        case "reboot":
            await commandDispatcher.RebootAsync(parts[1]);
            break;
case "history":

{
    var history =
        await historyService.GetAsync(parts[1], 20);

    if (history.Count == 0)
    {
        Console.WriteLine("No commands found.");
        break;
    }

    Console.WriteLine();

    Console.WriteLine("=========== COMMAND HISTORY ===========");

    foreach (var cmd in history)
    {
        Console.WriteLine(
            $"{cmd.SentAt:yyyy-MM-dd HH:mm:ss} UTC");

        Console.WriteLine(
            $"Command   : {cmd.Command}");

        Console.WriteLine(
            $"Status    : {cmd.Status}");

        Console.WriteLine(
            $"Flag      : {cmd.ServerFlag}");

        Console.WriteLine(
            $"Response  : {cmd.Response}");

        Console.WriteLine(
            $"Protocol  : {cmd.Protocol}");

        Console.WriteLine("---------------------------------------");
    }

    break;
}

        default:
            Console.WriteLine("Unknown command");
            break;
    }
}

static void PrintRegistry(
    DeviceRegistry registry)
{
    Console.WriteLine();

    Console.WriteLine(
        "=========== DEVICE REGISTRY ===========");

    var devices =
        registry.Devices.ToList();



    Console.WriteLine(
        $"Online Devices : {devices.Count(d => d.Online)}");


    Console.WriteLine();

    foreach (var device in devices.OrderBy(d => d.Imei))
    {
        Console.WriteLine(
            $"IMEI       : {device.Imei}");

        Console.WriteLine(
            $"Online     : {device.Online}");

        Console.WriteLine(
            $"Connection : {device.ConnectionId}");

        Console.WriteLine(
            $"Packets    : {device.PacketCount}");

        Console.WriteLine(
            $"Last Seen  : {device.LastSeen:yyyy-MM-dd HH:mm:ss} UTC");

        if (device.LastPosition != null)
        {
            Console.WriteLine(
                $"Latitude   : {device.LastPosition.Latitude:F6}");

            Console.WriteLine(
                $"Longitude  : {device.LastPosition.Longitude:F6}");

            Console.WriteLine(
                $"Speed      : {device.LastPosition.Speed:F0} km/h");
        }


        Console.WriteLine(
            "---------------------------------------");
    }

    Console.WriteLine(
        "=======================================");


    Console.WriteLine();
}