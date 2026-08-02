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

// مهمة استقبال الردود من السيرفر
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
            Console.WriteLine($"SERVER -> {Convert.ToHexString(packet)}");

            if (packet.Length < 8)
                continue;

            // التحقق من رقم البروتوكول (Command Response = 0x80)
         // إذا كانت الحزمة أمر من السيرفر (0x80)
            if (packet.Length >= 11 && packet[3] == 0x80)
            {
                ushort flag = BinaryPrimitives.ReadUInt16BigEndian(packet.AsSpan(5, 2));

                Console.WriteLine($"SERVER COMMAND FLAG = {flag}");

                var response = Gt06CommandResponseEncoder.BuildCommandResponse(flag);

                Console.WriteLine($"SEND RESPONSE -> {Convert.ToHexString(response)}");

                await stream.WriteAsync(response, cts.Token);

                continue;
            }

            // أي رد آخر فقط اطبعه
            Console.WriteLine($"SERVER -> {Convert.ToHexString(packet)}");
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Receive task cancelled");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Receive error: {ex.Message}");
    }
});

await Task.Delay(500);

try
{
    // ================================
    // إرسال حزمة تسجيل الدخول
    // ================================
    string imei = "011724359011720";
    var loginPacket = BuildLoginPacket(imei, serial++);
    
    Console.WriteLine($"SEND LOGIN: {Convert.ToHexString(loginPacket)}");
    await stream.WriteAsync(loginPacket, cts.Token);
    
    await Task.Delay(1000);

    // ================================
    // إرسال حزمة النبض (Heartbeat)
    // ================================
    var heartbeatPacket = BuildHeartbeatPacket(serial++);
    
    Console.WriteLine($"SEND HEARTBEAT: {Convert.ToHexString(heartbeatPacket)}");
    await stream.WriteAsync(heartbeatPacket, cts.Token);
    
    await Task.Delay(1000);

    // ================================
    // محاكاة حركة المركبة
    // ================================
    var route = new[]
    {
        new { Lat = 24.713600, Lon = 46.675300, Speed = (byte)0 },
        new { Lat = 24.713900, Lon = 46.676000, Speed = (byte)20 },
        new { Lat = 24.714300, Lon = 46.676800, Speed = (byte)40 },
        new { Lat = 24.714900, Lon = 46.677500, Speed = (byte)60 },
        new { Lat = 24.715500, Lon = 46.678300, Speed = (byte)50 }
    };

    Console.WriteLine("Starting GPS simulation...");
    
    foreach (var point in route)
    {
        bool isNorth = point.Lat >= 0;
        bool isEast = point.Lon >= 0;
        
        var gpsPacket = BuildGpsPacket(
            serial++,
            Math.Abs(point.Lat),
            Math.Abs(point.Lon),
            point.Speed,
            90,
            true,
            isNorth,
            isEast,
            true
        );

        Console.WriteLine($"SEND GPS: {point.Lat:F6}, {point.Lon:F6} Speed={point.Speed} km/h");
        
        await stream.WriteAsync(gpsPacket, cts.Token);
        await Task.Delay(3000, cts.Token);
    }

    Console.WriteLine("GPS simulation completed");
    Console.WriteLine("Press any key to stop...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
finally
{
    cts.Cancel();
    try { await receiveTask; }
    catch { }
    
    client.Close();
    Console.WriteLine("Disconnected");
}

// ========================================
// دوال بناء الحزم
// ========================================

static byte[] BuildLoginPacket(string imei, ushort serial)
{
    using var ms = new MemoryStream();

    // Header
    ms.WriteByte(0x78);
    ms.WriteByte(0x78);

    // Length
    ms.WriteByte(0x0D);

    // Protocol = Login
    ms.WriteByte(0x01);

    // ======================================
    // IMEI (15 digits -> 8 BCD bytes)
    // ======================================

    if (imei.Length != 15)
        throw new ArgumentException("GT06 IMEI must contain exactly 15 digits.");

    Span<byte> imeiBytes = stackalloc byte[8];

    // أول بايت: النبلة العليا = 0 ، النبلة السفلى = أول رقم
    imeiBytes[0] = (byte)(imei[0] - '0');

    int index = 1;

    for (int i = 1; i < imei.Length; i += 2)
    {
        byte high = (byte)(imei[i] - '0');
        byte low = (byte)(imei[i + 1] - '0');

        imeiBytes[index++] = (byte)((high << 4) | low);
    }

    ms.Write(imeiBytes);

    // Serial
    WriteUInt16(ms, serial);

    // CRC
    WriteCrc(ms);

    // Tail
    ms.WriteByte(0x0D);
    ms.WriteByte(0x0A);

    return ms.ToArray();
}
static byte[] BuildHeartbeatPacket(ushort serial)
{
    using var ms = new MemoryStream();
    
    // Header
    ms.WriteByte(0x78);
    ms.WriteByte(0x78);
    
    // Length: Protocol(1) + Serial(2) = 3 = 0x03
    // لكن في GT06 القياسي Heartbeat Length = 0x05
    ms.WriteByte(0x05);
    
    // Protocol Number (Heartbeat = 0x13)
    ms.WriteByte(0x13);
    
    // Serial Number
    WriteUInt16(ms, serial);
    
    // CRC
    WriteCrc(ms);
    
    // Stop Bits
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
    
    // Header
    ms.WriteByte(0x78);
    ms.WriteByte(0x78);
    
    // Length: Protocol(1) + DateTime(6) + GPS Info(1) + Lat(4) + Lon(4) + Speed(1) + Course(2) + Serial(2)
    // = 1 + 6 + 1 + 4 + 4 + 1 + 2 + 2 = 21 = 0x15
    ms.WriteByte(0x15);
    
    // Protocol Number (GPS = 0x12)
    ms.WriteByte(0x12);
    
    // Date & Time (UTC)
    var now = DateTime.UtcNow;
    ms.WriteByte((byte)(now.Year - 2000));
    ms.WriteByte((byte)now.Month);
    ms.WriteByte((byte)now.Day);
    ms.WriteByte((byte)now.Hour);
    ms.WriteByte((byte)now.Minute);
    ms.WriteByte((byte)now.Second);
    
    // GPS Information (0xC0 = 12 satellites + Valid)
    ms.WriteByte(0xC0);
    
    // Latitude (absolute value × 1800000)
    uint lat = (uint)(latitude * 1800000);
    WriteUInt32(ms, lat);
    
    // Longitude (absolute value × 1800000)
    uint lon = (uint)(longitude * 1800000);
    WriteUInt32(ms, lon);
    
    // Speed
    ms.WriteByte(speed);
    
    // Course & Status
    ushort courseStatus = (ushort)(course & 0x03FF);
    
    if (gpsFixed)
        courseStatus |= 0x0400;
    
    if (!isEast)
        courseStatus |= 0x0800;
    
    if (!isNorth)
        courseStatus |= 0x1000;
    
    if (realTime)
        courseStatus |= 0x2000;
    
    WriteUInt16(ms, courseStatus);
    
    // Serial Number
    WriteUInt16(ms, serial);
    
    // CRC
    WriteCrc(ms);
    
    // Stop Bits
    ms.WriteByte(0x0D);
    ms.WriteByte(0x0A);
    
    return ms.ToArray();
}

static void WriteUInt16(MemoryStream ms, ushort value)
{
    Span<byte> buffer = stackalloc byte[2];
    BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
    ms.Write(buffer);
}

static void WriteUInt32(MemoryStream ms, uint value)
{
    Span<byte> buffer = stackalloc byte[4];
    BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
    ms.Write(buffer);
}

static void WriteCrc(MemoryStream ms)
{
    // حساب CRC من البايت الثالث (بعد 0x78 0x78) حتى نهاية البيانات الحالية
    // (قبل إضافة CRC و Stop Bits)
    var data = ms.ToArray();
    var crcData = data[2..]; // استبعاد Header فقط
    
    ushort crc = Crc16.Compute(crcData);
    WriteUInt16(ms, crc);
}