using System.Net;
using System.Net.Sockets;
using Tracking.SDK.Interfaces;

namespace Tracking.Network.Models;

public sealed class ClientSession : IDeviceSession
{
    public Guid Id { get; } = Guid.NewGuid();

    public TcpClient Client { get; }

    public NetworkStream Stream => Client.GetStream();

    public DateTime ConnectedAt { get; } = DateTime.UtcNow;

    public string? DeviceId { get; set; }

    public string? ProtocolId { get; set; }

    public ClientSession(TcpClient client)
    {
        Client = client;
    }

    // IDeviceSession
    public string ConnectionId => Id.ToString();

    public EndPoint RemoteEndPoint =>
        Client.Client.RemoteEndPoint!;

    public async ValueTask SendAsync(ReadOnlyMemory<byte> data)
    {
        await Stream.WriteAsync(data);
    }

    public ValueTask CloseAsync()
    {
        Client.Close();
        return ValueTask.CompletedTask;
    }
}