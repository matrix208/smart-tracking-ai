using Tracking.Commands.Models;
using Tracking.Commands.Stores;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;
using CommandResult = Tracking.SDK.Models.CommandResult;
using CommandCommand = Tracking.Commands.Models.DeviceCommand;

namespace Tracking.Commands.Lifecycle;

public sealed class CommandLifecycleService
{
    private readonly PendingCommandStore _pendingStore;
    private readonly TrackingDbContext _db;

    public CommandLifecycleService(
        PendingCommandStore pendingStore,
        TrackingDbContext db)
    {
        _pendingStore = pendingStore;
        _db = db;
    }

    public async Task RegisterAsync(
        CommandCommand command,
        string? protocol = null)
    {
        _pendingStore.Register(command);

        var entity = new CommandEntity
        {
            DeviceId = command.DeviceId,
            Command = command.Type.ToString(),
            ServerFlag = command.ServerFlag,
            SentAt = DateTime.UtcNow,
            Status = "Pending",
            Protocol = protocol
        };

        _db.Commands.Add(entity);

        await _db.SaveChangesAsync();
    }

    public async Task CompleteAsync(
        CommandResult result)
    {
        if (!_pendingStore.TryComplete(
                result,
                out var command))
        {
            return;
        }

        var entity =
            _db.Commands
               .OrderByDescending(x => x.Id)
               .FirstOrDefault(x =>
                    x.ServerFlag == result.ServerFlag);

        if (entity is null)
            return;

        entity.CompletedAt = DateTime.UtcNow;
        entity.Status = result.Success
            ? "Success"
            : "Failed";
        entity.Response = result.Response;

        await _db.SaveChangesAsync();
    }

    public async Task TimeoutAsync(
        CommandCommand command)
    {
        var entity =
            _db.Commands
               .OrderByDescending(x => x.Id)
               .FirstOrDefault(x =>
                    x.ServerFlag == command.ServerFlag);

        if (entity is null)
            return;

        entity.CompletedAt = DateTime.UtcNow;
        entity.Status = "TimedOut";

        await _db.SaveChangesAsync();
    }
}