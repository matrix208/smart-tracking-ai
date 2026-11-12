using BinaryReader = Tracking.Protocol.Binary.BinaryReader;
using Tracking.Plugin.GT06.Protocol.Messages;

namespace Tracking.Plugin.GT06.Protocol.Decoders;

public sealed class CommandAckDecoder
{
    public CommandResponseMessage Decode(BinaryReader reader)
    {
        ushort serverFlag = reader.ReadUInt16BE();

        return new CommandResponseMessage
        {
            ServerFlag = serverFlag,
            Success = true,
            Text = "ACK"
        };
    }
}