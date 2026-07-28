using Tracking.Core.Services;
using Tracking.Network.Models;
using Tracking.Network.Servers;
using Tracking.Pipeline;
using Tracking.PluginLoader.Services;
using Tracking.SDK.Enums;

var loader = new PluginLoader();

var plugins = await loader.LoadAsync("Plugins");

Console.WriteLine("Plugins Loaded");
Console.WriteLine("----------------");

foreach (var plugin in plugins)
{
    Console.WriteLine(plugin.Manifest.Name);
}

Console.WriteLine();

var pipeline = new PacketPipeline(plugins);

var registry = new DeviceRegistry();
var deviceManager = new DeviceManager(registry);

var server = new TcpTrackingServer(5001);

server.PacketReceived += async (session, packet) =>
{
    var message = await pipeline.ProcessAsync(packet, session);

    if (message == null)
        return;

    // إذا كانت الرسالة GPS ولم تحمل DeviceId، خذه من الجلسة
    if (message.Type == MessageType.Position &&
        string.IsNullOrWhiteSpace(message.DeviceId) &&
        session is ClientSession clientSession &&
        !string.IsNullOrWhiteSpace(clientSession.DeviceId))
    {
        message = message with { DeviceId = clientSession.DeviceId };
    }

    await deviceManager.ProcessAsync(session, message);

    Console.WriteLine($"Decoded: {message.Type}");

    PrintRegistry(registry);
};

await server.StartAsync();

static void PrintRegistry(DeviceRegistry registry)
{
    Console.WriteLine();
    Console.WriteLine("=========== DEVICE REGISTRY ===========");

    var devices = registry.Devices.ToList();

    Console.WriteLine($"Online Devices : {devices.Count(d => d.Online)}");
    Console.WriteLine();

    foreach (var device in devices.OrderBy(d => d.Imei))
    {
        Console.WriteLine($"IMEI       : {device.Imei}");
        Console.WriteLine($"Online     : {device.Online}");
        Console.WriteLine($"Connection : {device.ConnectionId}");
        Console.WriteLine($"Packets    : {device.PacketCount}");
        Console.WriteLine($"Last Seen  : {device.LastSeen:yyyy-MM-dd HH:mm:ss} UTC");

        if (device.LastPosition != null)
        {
            Console.WriteLine($"Latitude   : {device.LastPosition.Latitude:F6}");
            Console.WriteLine($"Longitude  : {device.LastPosition.Longitude:F6}");
            Console.WriteLine($"Speed      : {device.LastPosition.Speed:F0} km/h");
        }

        Console.WriteLine("---------------------------------------");
    }

    Console.WriteLine("=======================================");
    Console.WriteLine();
}