using System.Buffers.Binary;
using System.Net.Sockets;
using Tracking.Plugin.GT06.Protocol.CRC;
using Tracking.Plugin.GT06.Protocol.Protocols;

Console.WriteLine("GT06 Simulator");

using var client = new TcpClient();

await client.ConnectAsync(
    "127.0.0.1",
    5001);

Console.WriteLine("Connected to server");

var stream = client.GetStream();

ushort serial = 1;


// ================================
// Send Login Packet
// ================================

string imei = "123456789012345";

var loginPacket = BuildLoginPacket(
    imei,
    serial);

Console.WriteLine(
    $"SEND LOGIN: {Convert.ToHexString(loginPacket)}");

await stream.WriteAsync(loginPacket);

await ReadResponse(stream);


// ================================
// Send Heartbeat Packet
// ================================

var heartbeatPacket = BuildHeartbeatPacket(
    serial++);

Console.WriteLine(
    $"SEND HEARTBEAT: {Convert.ToHexString(heartbeatPacket)}");

await stream.WriteAsync(heartbeatPacket);

await ReadResponse(stream);


// ================================
// Send GPS Packet
// ================================

// ================================
// Simulate Vehicle Movement
// ================================

var route = new[]
{
    new { Lat = 24.713600, Lon = 46.675300, Speed = (byte)0 },
    new { Lat = 24.713900, Lon = 46.676000, Speed = (byte)20 },
    new { Lat = 24.714300, Lon = 46.676800, Speed = (byte)40 },
    new { Lat = 24.714900, Lon = 46.677500, Speed = (byte)60 },
    new { Lat = 24.715500, Lon = 46.678300, Speed = (byte)50 }
};

foreach (var point in route)
{
    var gpsPacket = BuildGpsPacket(
        serial++,
        point.Lat,
        point.Lon,
        point.Speed,
        90);

    Console.WriteLine(
        $"SEND GPS: {point.Lat},{point.Lon} Speed={point.Speed}");

    await stream.WriteAsync(gpsPacket);

    await Task.Delay(3000);
}


Console.WriteLine("Simulator finished");



static byte[] BuildLoginPacket(string imei, ushort serial)
{
    using var ms = new MemoryStream();

    ms.WriteByte(0x78);
    ms.WriteByte(0x78);

    ms.WriteByte(0x0D);

    ms.WriteByte((byte)Gt06MessageType.Login);

    var digits = imei.PadLeft(16, '0');

    for (int i = 0; i < 16; i += 2)
    {
        ms.WriteByte(
            byte.Parse(digits.Substring(i, 2)));
    }

    WriteUInt16(ms, serial);

    WriteCrc(ms);

    ms.WriteByte(0x0D);
    ms.WriteByte(0x0A);

    return ms.ToArray();
}



static byte[] BuildHeartbeatPacket(ushort serial)
{
    using var ms = new MemoryStream();

    ms.WriteByte(0x78);
    ms.WriteByte(0x78);

    ms.WriteByte(0x05);

    ms.WriteByte((byte)Gt06MessageType.Heartbeat);

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
    ushort course)
{
    using var ms = new MemoryStream();

    ms.WriteByte(0x78);
    ms.WriteByte(0x78);


    // Length:
    // Protocol + Time(6) + GPS info(1)
    // Lat(4) + Lon(4) + Speed(1)
    // Course(2) + Serial(2) + CRC(2)
    ms.WriteByte(0x1F);


    ms.WriteByte((byte)Gt06MessageType.GPS);


    // Date YY MM DD HH MM SS
    var now = DateTime.UtcNow;

    ms.WriteByte((byte)(now.Year - 2000));
    ms.WriteByte((byte)now.Month);
    ms.WriteByte((byte)now.Day);
    ms.WriteByte((byte)now.Hour);
    ms.WriteByte((byte)now.Minute);
    ms.WriteByte((byte)now.Second);


    // GPS information
    ms.WriteByte(0xC0);


    uint lat =
        (uint)(latitude * 1800000);

    uint lon =
        (uint)(longitude * 1800000);


    WriteUInt32(ms, lat);
    WriteUInt32(ms, lon);


    ms.WriteByte(speed);


    WriteUInt16(ms, course);


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



static void WriteCrc(MemoryStream ms)
{
    var data = ms.ToArray()[2..];

    ushort crc = Crc16.Compute(data);

    WriteUInt16(ms, crc);
}



static async Task ReadResponse(NetworkStream stream)
{
    var buffer = new byte[256];

    int read =
        await stream.ReadAsync(buffer);

    if (read > 0)
    {
        Console.WriteLine(
            $"RECV: {Convert.ToHexString(buffer[..read])}");
    }
}