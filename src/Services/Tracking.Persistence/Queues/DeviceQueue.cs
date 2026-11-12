using System.Collections.Concurrent;
using Tracking.Storage.Entities;

namespace Tracking.Persistence.Queues;

public sealed class DeviceQueue
{
    private readonly ConcurrentQueue<DeviceEntity> _queue = new();


    public void Enqueue(DeviceEntity device)
    {
        _queue.Enqueue(device);
    }


    public bool TryDequeue(
        out DeviceEntity? device)
    {
        return _queue.TryDequeue(
            out device);
    }


    public int Count =>
        _queue.Count;
}