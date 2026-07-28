using System.Net;

namespace Tracking.SDK.Interfaces;

public interface IDeviceSession
{
    string ConnectionId { get; }

    EndPoint RemoteEndPoint { get; }

    ValueTask SendAsync(ReadOnlyMemory<byte> data);

    ValueTask CloseAsync();
}