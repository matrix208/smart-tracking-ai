namespace Tracking.Network.Readers;

public sealed class PacketReader
{
    public byte[] Read(byte[] buffer, int length)
    {
        var packet = new byte[length];

        Buffer.BlockCopy(buffer, 0, packet, 0, length);

        return packet;
    }
}