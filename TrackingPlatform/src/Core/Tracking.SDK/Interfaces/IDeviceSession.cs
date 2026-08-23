using System.Net;

namespace Tracking.SDK.Interfaces;

public interface IDeviceSession
{
    string ConnectionId { get; }

    EndPoint RemoteEndPoint { get; }

    string? DeviceId { get; set; }

    string? ProtocolId { get; set; }

    ValueTask SendAsync(ReadOnlyMemory<byte> data);

    ValueTask CloseAsync();
}