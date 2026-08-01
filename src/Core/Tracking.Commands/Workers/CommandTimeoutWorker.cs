using Microsoft.Extensions.Hosting;
using Tracking.Commands.Stores;

namespace Tracking.Commands.Workers;

public sealed class CommandTimeoutWorker : BackgroundService
{
    private readonly PendingCommandStore _pendingStore;

    public CommandTimeoutWorker(
        PendingCommandStore pendingStore)
    {
        _pendingStore = pendingStore;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        Console.WriteLine(
            "Command Timeout Worker Started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var expired =
                _pendingStore.RemoveExpired();

            foreach (var command in expired)
            {
                Console.WriteLine(
                    $"[Command] Timeout -> {command.DeviceId} Flag={command.ServerFlag}");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(1),
                stoppingToken);
        }
    }
}