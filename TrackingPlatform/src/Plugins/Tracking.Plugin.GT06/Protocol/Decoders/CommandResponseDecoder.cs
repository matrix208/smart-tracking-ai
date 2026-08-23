using System.Text;
using BinaryReader = Tracking.Protocol.Binary.BinaryReader;
using Tracking.Plugin.GT06.Protocol.Messages;

namespace Tracking.Plugin.GT06.Protocol.Decoders;

public sealed class CommandResponseDecoder
{
    public CommandResponseMessage Decode(
        BinaryReader reader)
    {
        // المتبقي:
        // Text + Serial + CRC
        int payloadLength = reader.Remaining - 2;

        if (payloadLength < 2)
            throw new InvalidOperationException(
                "Invalid command response.");

        int textLength = payloadLength - 2;

        string text =
            Encoding.ASCII.GetString(
                reader.ReadBytes(textLength));

        ushort flag =
            reader.ReadUInt16BE();

        return new CommandResponseMessage
        {
            ServerFlag = flag,
            Success = text.StartsWith(
                "OK",
                StringComparison.OrdinalIgnoreCase),
            Text = text
        };
    }
}