using Microsoft.Extensions.Hosting;
using Tracking.Commands.Channels;
using Tracking.Core.Services;
using Tracking.PluginManager.Services;
using Tracking.Commands.Sequence;

namespace Tracking.Commands.Workers;

public sealed class CommandWorker : BackgroundService
{
    private readonly CommandChannel _channel;
    private readonly DeviceRegistry _registry;
    private readonly ProtocolPluginManager _pluginManager;
    private readonly CommandSequence _sequence;

  public CommandWorker(
    CommandChannel channel,
    DeviceRegistry registry,
    ProtocolPluginManager pluginManager,
    CommandSequence sequence)
{
    _channel = channel;
    _registry = registry;
    _pluginManager = pluginManager;
    _sequence = sequence;
}

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        Console.WriteLine("Command Worker Started");

        await foreach (var command in _channel.ReadAllAsync(stoppingToken))
        {
            try
            {
                command.ServerFlag = _sequence.Next();
                
                if (!_registry.TryGet(
                        command.DeviceId,
                        out var device))
                {
                    Console.WriteLine(
                        $"[Command] Device not found -> {command.DeviceId}");
                    continue;
                }
Console.WriteLine(
    $"WORKER Session={device?.Session?.GetHashCode()} Protocol={device?.Session?.ProtocolId}");
    
                if (device?.Session?.ProtocolId is null)
                {
                    Console.WriteLine(
                        $"[Command] Protocol not detected -> {command.DeviceId}");
                    continue;
                }

                var plugin =
                    _pluginManager.Get(
                        device.Session.ProtocolId);

                if (plugin is null)
                {
                    Console.WriteLine(
                        $"[Command] Plugin not found -> {device.Session.ProtocolId}");
                    continue;
                }
var sdkCommand =
    new Tracking.SDK.Models.DeviceCommand
    {
        DeviceId = command.DeviceId,
        Name = command.Type.ToString(),
        ServerFlag = command.ServerFlag
    };

for (int i = 0; i < command.Parameters.Length; i++)
{
    sdkCommand.Parameters.Add(
        $"arg{i}",
        command.Parameters[i]);
}

var packet =
    await plugin.EncodeAsync(
        sdkCommand);

                var sent =
                    await _registry.SendAsync(
                        command.DeviceId,
                        packet);

                if (sent)
                {
                    Console.WriteLine(
                        $"[Command] Sent -> {command.DeviceId}");
                }
                else
                {
                    Console.WriteLine(
                        $"[Command] Device Offline -> {command.DeviceId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[Command] Error: {ex.Message}");
            }
        }
    }
}