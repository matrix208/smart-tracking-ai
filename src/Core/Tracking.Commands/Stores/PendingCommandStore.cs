using System.Collections.Concurrent;
using Tracking.Commands.Models;
using CommandResult = Tracking.SDK.Models.CommandResult;

namespace Tracking.Commands.Stores;

public sealed class PendingCommandStore
{
    private sealed record PendingEntry(
        DeviceCommand Command,
        DateTime CreatedAt);

    private readonly ConcurrentDictionary<uint, PendingEntry> _pending = new();

    public void Register(DeviceCommand command)
    {
        _pending[command.ServerFlag] =
            new PendingEntry(
                command,
                DateTime.UtcNow);
    }

    public bool TryGet(
        uint serverFlag,
        out DeviceCommand? command)
    {
        if (_pending.TryGetValue(serverFlag, out var entry))
        {
            command = entry.Command;
            return true;
        }

        command = null;
        return false;
    }

    public bool TryComplete(
        uint serverFlag,
        out DeviceCommand? command)
    {
        if (_pending.TryRemove(serverFlag, out var entry))
        {
            command = entry.Command;
            return true;
        }

        command = null;
        return false;
    }

    public bool TryComplete(
        CommandResult result,
        out DeviceCommand? command)
    {
        return TryComplete(
            result.ServerFlag,
            out command);
    }

    /// <summary>
    /// إزالة جميع الأوامر التي تجاوزت المهلة.
    /// </summary>
    public IReadOnlyList<DeviceCommand> RemoveExpired()
    {
        var now = DateTime.UtcNow;

        var expired =
            new List<DeviceCommand>();

        foreach (var pair in _pending)
        {
            var entry = pair.Value;

            if (now - entry.CreatedAt < entry.Command.Timeout)
                continue;

            if (_pending.TryRemove(pair.Key, out var removed))
            {
                expired.Add(removed.Command);
            }
        }

        return expired;
    }

    public int Count => _pending.Count;
}