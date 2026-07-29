using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Tracking.Network.Models;
using Tracking.Network.Readers;

namespace Tracking.Network.Servers;

public sealed class TcpTrackingServer
{
    private readonly TcpListener _listener;

    private readonly PacketReader _packetReader = new();


    // جميع الجلسات الحالية
    private readonly ConcurrentDictionary<Guid, ClientSession> _sessions = new();



    // عدد الاتصالات الحالية
    public int ConnectedCount =>
        _sessions.Count;



    // استقبال Packet
    public event Func<ClientSession, ReadOnlyMemory<byte>, Task>? PacketReceived;



    // فصل جهاز
    public event Func<ClientSession, Task>? ClientDisconnected;



    public TcpTrackingServer(int port)
    {
        _listener =
            new TcpListener(
                IPAddress.Any,
                port);
    }



    public async Task StartAsync(
        CancellationToken cancellationToken = default)
    {
        _listener.Start(
            1000);


        Console.WriteLine(
            $"TCP Server listening on {_listener.LocalEndpoint}");



        while (!cancellationToken.IsCancellationRequested)
        {
            var client =
                await _listener.AcceptTcpClientAsync(
                    cancellationToken);



            var session =
                new ClientSession(client);



            _sessions.TryAdd(
                session.Id,
                session);



            Console.WriteLine(
                $"Client Connected: {session.Id}");


            _ = HandleClientAsync(
                session,
                cancellationToken);
        }
    }



    private async Task HandleClientAsync(
        ClientSession session,
        CancellationToken cancellationToken)
    {
        try
        {
            var buffer =
                new byte[8192];



            while (!cancellationToken.IsCancellationRequested)
            {
                var read =
                    await session.Stream.ReadAsync(
                        buffer,
                        cancellationToken);



                if (read == 0)
                    break;



                var packet =
                    _packetReader.Read(
                        buffer,
                        read);



                if (PacketReceived != null)
                {
                    await PacketReceived(
                        session,
                        packet);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Session Error {session.Id}: {ex.Message}");
        }
        finally
        {
            _sessions.TryRemove(
                session.Id,
                out _);



            try
            {
                session.Client.Close();
            }
            catch
            {
            }



            if (ClientDisconnected != null)
            {
                await ClientDisconnected(
                    session);
            }



            Console.WriteLine(
                $"Client Disconnected: {session.Id}");


            Console.WriteLine(
                $"Active Connections: {_sessions.Count}");
        }
    }
}