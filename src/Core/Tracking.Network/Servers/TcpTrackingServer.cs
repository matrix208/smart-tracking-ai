using System.Net;
using System.Net.Sockets;
using Tracking.Network.Models;
using Tracking.Network.Readers;

namespace Tracking.Network.Servers;

public sealed class TcpTrackingServer
{
    private readonly TcpListener _listener;
    private readonly PacketReader _packetReader = new();

    // يحدث عند استقبال Packet كاملة
    public event Func<ClientSession, ReadOnlyMemory<byte>, Task>? PacketReceived;

    public TcpTrackingServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public async Task StartAsync(
        CancellationToken cancellationToken = default)
    {
        _listener.Start();

        Console.WriteLine(
            $"TCP Server listening on {_listener.LocalEndpoint}");

        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(
                cancellationToken);

            _ = HandleClientAsync(
                new ClientSession(client),
                cancellationToken);
        }
    }

    private async Task HandleClientAsync(
        ClientSession session,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Client Connected: {session.Id}");

        try
        {
            var buffer = new byte[4096];

            while (!cancellationToken.IsCancellationRequested)
            {
                var read = await session.Stream.ReadAsync(
                    buffer,
                    cancellationToken);

                if (read == 0)
                    break;

                var packet = _packetReader.Read(buffer, read);

                Console.WriteLine(
                    $"Packet ({packet.Length} bytes): {Convert.ToHexString(packet)}");

                // إشعار أي مشترك بوصول Packet
                if (PacketReceived != null)
                {
                    await PacketReceived(session, packet);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Session Error: {ex.Message}");
        }
        finally
        {
            session.Client.Close();

            Console.WriteLine(
                $"Client Disconnected: {session.Id}");
        }
    }
}