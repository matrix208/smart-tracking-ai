using System.Buffers;

namespace Tracking.Network.Readers;

/// <summary>
/// Accumulates bytes received from a TCP stream and extracts complete
/// Teltonika/GT06-compatible frames.
///
/// TCP is a byte stream: one ReadAsync does NOT necessarily equal one
/// application packet. A frame may be fragmented across reads, or
/// multiple frames may arrive in one read.
///
/// The reader currently recognizes:
/// 1. Teltonika IMEI handshake:
///      [UInt16 BE length][ASCII IMEI]
///
/// 2. Teltonika AVL:
///      [4-byte zero preamble][UInt32 BE data length][data][4-byte CRC]
///
/// 3. GT06:
///      [0x7878/0x7979 ... length encoded by the GT06 protocol]
///
/// Unknown/incomplete data is retained until more bytes arrive.
/// </summary>
public sealed class PacketReader
{
    private byte[] _buffer = new byte[16 * 1024];
    private int _length;

    public IReadOnlyList<byte[]> Read(byte[] buffer, int length)
    {
        if (length <= 0)
            return [];

        EnsureCapacity(_length + length);

        Buffer.BlockCopy(
            buffer,
            0,
            _buffer,
            _length,
            length);

        _length += length;

        var packets = new List<byte[]>();

        while (true)
        {
            var packet = TryReadPacket();

            if (packet is null)
                break;

            packets.Add(packet);
        }

        return packets;
    }

    private byte[]? TryReadPacket()
    {
        if (_length < 2)
            return null;

        // -----------------------------------------------------
        // Teltonika IMEI handshake
        // -----------------------------------------------------
        //
        // [2-byte big-endian length][ASCII IMEI]
        //
        var imeiLength = (_buffer[0] << 8) | _buffer[1];

        if (imeiLength >= 10 &&
            imeiLength <= 17)
        {
            var totalLength = 2 + imeiLength;

            if (_length < totalLength)
                return null;

            var validImei = true;

            for (var i = 2; i < totalLength; i++)
            {
                if (_buffer[i] < (byte)'0' ||
                    _buffer[i] > (byte)'9')
                {
                    validImei = false;
                    break;
                }
            }

            if (validImei)
                return TakePacket(totalLength);
        }

        // -----------------------------------------------------
        // Teltonika AVL
        // -----------------------------------------------------
        //
        // [00000000][4-byte data length][data][4-byte CRC]
        //
        if (_length >= 8 &&
            _buffer[0] == 0x00 &&
            _buffer[1] == 0x00 &&
            _buffer[2] == 0x00 &&
            _buffer[3] == 0x00)
        {
            var dataLength =
                (_buffer[4] << 24) |
                (_buffer[5] << 16) |
                (_buffer[6] << 8) |
                _buffer[7];

            // Protect the TCP reader from corrupt/unreasonable lengths.
            const int maxTeltonikaDataLength = 1024 * 1024;

            if (dataLength < 3 ||
                dataLength > maxTeltonikaDataLength)
            {
                DropOneByte();
                return null;
            }

            var totalLength =
                8 + dataLength + 4;

            if (_length < totalLength)
                return null;

            return TakePacket(totalLength);
        }

        // -----------------------------------------------------
        // GT06
        // -----------------------------------------------------
        //
        // GT06 commonly begins with:
        //   78 78
        // or:
        //   79 79
        //
        // The packet length is encoded in the frame after the
        // start bytes. Keep the existing protocol reader behavior
        // compatible while allowing TCP fragmentation.
        //
        if (_length >= 3 &&
            ((_buffer[0] == 0x78 && _buffer[1] == 0x78) ||
             (_buffer[0] == 0x79 && _buffer[1] == 0x79)))
        {
            var lengthField = _buffer[2];

            // GT06 packet:
            //
            // start(2)
            // length(1)
            // payload(lengthField)
            //
            // The existing GT06 implementation expects the complete
            // protocol frame. The common total size is:
            //
            // 2 + 1 + lengthField
            //
            var totalLength = 3 + lengthField;

            if (totalLength < 7 ||
                totalLength > 65535)
            {
                DropOneByte();
                return null;
            }

            if (_length < totalLength)
                return null;

            return TakePacket(totalLength);
        }

        // -----------------------------------------------------
        // Unknown byte
        // -----------------------------------------------------
        //
        // Search for the next possible protocol boundary instead
        // of allowing invalid data to permanently block the stream.
        //
        DropOneByte();

        return null;
    }

    private byte[] TakePacket(int length)
    {
        var packet = new byte[length];

        Buffer.BlockCopy(
            _buffer,
            0,
            packet,
            0,
            length);

        var remaining = _length - length;

        if (remaining > 0)
        {
            Buffer.BlockCopy(
                _buffer,
                length,
                _buffer,
                0,
                remaining);
        }

        _length = remaining;

        return packet;
    }

    private void DropOneByte()
    {
        if (_length <= 0)
            return;

        if (_length > 1)
        {
            Buffer.BlockCopy(
                _buffer,
                1,
                _buffer,
                0,
                _length - 1);
        }

        _length--;
    }

    private void EnsureCapacity(int required)
    {
        if (required <= _buffer.Length)
            return;

        var newSize = _buffer.Length;

        while (newSize < required)
            newSize *= 2;

        Array.Resize(
            ref _buffer,
            newSize);
    }
}
