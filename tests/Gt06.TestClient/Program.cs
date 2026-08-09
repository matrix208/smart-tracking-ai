using System.Net.Sockets;
using Tracking.Plugin.GT06.Protocol.Encoders;

var imei = "001172435901172";

var loginEncoder = new LoginEncoder();

var loginPacket = loginEncoder.Encode(
    imei,
    1);

Console.WriteLine(
    $"Login Packet: {Convert.ToHexString(loginPacket.Span)}");


using var client = new TcpClient();

await client.ConnectAsync(
    "127.0.0.1",
    5001);

var stream = client.GetStream();


// =============================
// Send Login
// =============================

await stream.WriteAsync(loginPacket);

Console.WriteLine(
    "GT06 Login Packet Sent");


await Task.Delay(1000);

// GPS
var gpsEncoder = new GpsEncoder();
var gpsPacket = gpsEncoder.Encode(
    DateTime.UtcNow,
    24.7136,
    46.6753,
    50,
    90,
    2);

Console.WriteLine(
    $"GPS Packet: {Convert.ToHexString(gpsPacket.Span)}");

await stream.WriteAsync(gpsPacket);

Console.WriteLine(
    "GT06 GPS Packet Sent");

await Task.Delay(10000);