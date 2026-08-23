using System.Buffers.Binary;
using System.Net.Sockets;
using Tracking.Plugin.GT06.Protocol.CRC;
using Tracking.Plugin.GT06.Protocol.Protocols;
using Tracking.Plugin.GT06.Protocol.Encoders;

Console.WriteLine("GT06 Simulator");
Console.WriteLine("==============");

using var client = new TcpClient();
using var cts = new CancellationTokenSource();

await client.ConnectAsync("127.0.0.1", 5001);
Console.WriteLine("Connected to server");

var stream = client.GetStream();
ushort serial = 1;


// استقبال ردود السيرفر
var receiveTask = Task.Run(async () =>
{
    var buffer = new byte[1024];

    try
    {
        while (!cts.Token.IsCancellationRequested)
        {
            int read = await stream.ReadAsync(buffer, cts.Token);

            if (read == 0)
            {
                Console.WriteLine("Server disconnected");
                break;
            }

            var packet = buffer[..read];

            Console.WriteLine(
                $"SERVER -> {Convert.ToHexString(packet)}");


            if (packet.Length < 8)
                continue;


            // Command من السيرفر
            if (packet.Length >= 11 &&
                packet[3] == 0x80)
            {
                ushort flag =
                    BinaryPrimitives.ReadUInt16BigEndian(
                        packet.AsSpan(5, 2));


                Console.WriteLine(
                    $"SERVER COMMAND FLAG = {flag}");


                var response =
                    Gt06CommandResponseEncoder
                        .BuildCommandResponse(flag);


                Console.WriteLine(
                    $"SEND RESPONSE -> {Convert.ToHexString(response)}");


                await stream.WriteAsync(
                    response,
                    cts.Token);

                continue;
            }
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine(
            "Receive task cancelled");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Receive error: {ex.Message}");
    }
});


await Task.Delay(500);


try
{
    // ================================
    // Login
    // ================================

    string imei = "011724359011720";

    var loginPacket =
        BuildLoginPacket(
            imei,
            serial++);


    Console.WriteLine(
        $"SEND LOGIN: {Convert.ToHexString(loginPacket)}");


    await stream.WriteAsync(
        loginPacket,
        cts.Token);


    await Task.Delay(1000);



    // ================================
    // Heartbeat
    // ================================

    var heartbeatPacket =
        BuildHeartbeatPacket(
            serial++);


    Console.WriteLine(
        $"SEND HEARTBEAT: {Convert.ToHexString(heartbeatPacket)}");


    await stream.WriteAsync(
        heartbeatPacket,
        cts.Token);


    await Task.Delay(1000);



    // ================================
    // GPS Simulation
    // ================================

    var route = new[]
    {
        new { Lat = 24.713600, Lon = 46.675300, Speed = (byte)0 },
        new { Lat = 24.713900, Lon = 46.676000, Speed = (byte)20 },
        new { Lat = 24.714300, Lon = 46.676800, Speed = (byte)40 },
        new { Lat = 24.714900, Lon = 46.677500, Speed = (byte)60 },
        new { Lat = 24.715500, Lon = 46.678300, Speed = (byte)50 }
    };


    Console.WriteLine(
        "Starting GPS simulation...");


    foreach (var point in route)
    {
        var gpsPacket =
            BuildGpsPacket(
                serial++,
                point.Lat,
                point.Lon,
                point.Speed,
                90,
                true,
                true,
                true,
                true);


        Console.WriteLine(
            $"SEND GPS: {point.Lat:F6}, {point.Lon:F6} Speed={point.Speed} km/h");


        await stream.WriteAsync(
            gpsPacket,
            cts.Token);


        await Task.Delay(
            3000,
            cts.Token);
    }


    Console.WriteLine(
        "GPS simulation completed");


    // ================================
    // Alarm Simulation
    // ================================

    var alarmPacket =
        BuildAlarmPacket(
            serial++);


    Console.WriteLine(
        $"SEND ALARM: {Convert.ToHexString(alarmPacket)}");


    await stream.WriteAsync(
        alarmPacket,
        cts.Token);


    Console.WriteLine(
        "Alarm sent");


    Console.WriteLine(
        "Press any key to stop...");


    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine(
        $"Error: {ex.Message}");
}
finally
{
    cts.Cancel();

    try
    {
        await receiveTask;
    }
    catch
    {
    }

    client.Close();

    Console.WriteLine(
        "Disconnected");
}


// ========================================
// Packet Builders
// ========================================

static byte[] BuildLoginPacket(
    string imei,
    ushort serial)
{
    using var ms = new MemoryStream();

    ms.WriteByte(0x78);
    ms.WriteByte(0x78);

    ms.WriteByte(0x0D);
    ms.WriteByte(0x01);

    Span<byte> imeiBytes = stackalloc byte[8];

    imeiBytes[0] =
        (byte)(imei[0] - '0');

    int index = 1;

    for (int i = 1; i < imei.Length; i += 2)
    {
        byte high =
            (byte)(imei[i] - '0');

        byte low =
            (byte)(imei[i + 1] - '0');

        imeiBytes[index++] =
            (byte)((high << 4) | low);
    }

    ms.Write(imeiBytes);

    WriteUInt16(ms, serial);

    WriteCrc(ms);

    ms.WriteByte(0x0D);
    ms.WriteByte(0x0A);

    return ms.ToArray();
}


static byte[] BuildHeartbeatPacket(
    ushort serial)
{
    using var ms = new MemoryStream();

    ms.WriteByte(0x78);
    ms.WriteByte(0x78);

    ms.WriteByte(0x05);

    ms.WriteByte(0x13);

    WriteUInt16(ms, serial);

    WriteCrc(ms);

    ms.WriteByte(0x0D);
    ms.WriteByte(0x0A);

    return ms.ToArray();
}


static byte[] BuildGpsPacket(
    ushort serial,
    double latitude,
    double longitude,
    byte speed,
    ushort course,
    bool gpsFixed,
    bool isNorth,
    bool isEast,
    bool realTime)
{
    using var ms = new MemoryStream();

    ms.WriteByte(0x78);
    ms.WriteByte(0x78);

    ms.WriteByte(0x15);

    ms.WriteByte(0x12);

    var now = DateTime.UtcNow;

    ms.WriteByte((byte)(now.Year - 2000));
    ms.WriteByte((byte)now.Month);
    ms.WriteByte((byte)now.Day);
    ms.WriteByte((byte)now.Hour);
    ms.WriteByte((byte)now.Minute);
    ms.WriteByte((byte)now.Second);

    ms.WriteByte(0xC0);

    WriteUInt32(
        ms,
        (uint)(latitude * 1800000));

    WriteUInt32(
        ms,
        (uint)(longitude * 1800000));

    ms.WriteByte(speed);

    ushort status =
        (ushort)(course & 0x03FF);

    if (gpsFixed)
        status |= 0x0400;

    if (!isEast)
        status |= 0x0800;

    if (!isNorth)
        status |= 0x1000;

    if (realTime)
        status |= 0x2000;

    WriteUInt16(ms, status);

    WriteUInt16(ms, serial);

    WriteCrc(ms);

    ms.WriteByte(0x0D);
    ms.WriteByte(0x0A);

    return ms.ToArray();
}


static byte[] BuildAlarmPacket(
    ushort serial)
{
    using var ms = new MemoryStream();

    ms.WriteByte(0x78);
    ms.WriteByte(0x78);

    ms.WriteByte(0x05);

    ms.WriteByte(0x16);

    ms.WriteByte(0x01);

    WriteUInt16(ms, serial);

    WriteCrc(ms);

    ms.WriteByte(0x0D);
    ms.WriteByte(0x0A);

    return ms.ToArray();
}


static void WriteUInt16(
    MemoryStream ms,
    ushort value)
{
    Span<byte> buffer = stackalloc byte[2];

    BinaryPrimitives.WriteUInt16BigEndian(
        buffer,
        value);

    ms.Write(buffer);
}


static void WriteUInt32(
    MemoryStream ms,
    uint value)
{
    Span<byte> buffer = stackalloc byte[4];

    BinaryPrimitives.WriteUInt32BigEndian(
        buffer,
        value);

    ms.Write(buffer);
}


static void WriteCrc(
    MemoryStream ms)
{
    var data = ms.ToArray();

    var crcData = data[2..];

    ushort crc =
        Crc16.Compute(crcData);

    WriteUInt16(ms, crc);
}