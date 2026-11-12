using BinaryReader = Tracking.Protocol.Binary.BinaryReader;
using Tracking.Plugin.GT06.Protocol.Models;

namespace Tracking.Plugin.GT06.Protocol.Decoders;

public sealed class LoginDecoder
{
    public LoginMessage Decode(BinaryReader reader)
    {
        string imei = reader.ReadImei();

        ushort serial = reader.ReadUInt16BE();

        return new LoginMessage
        {
            Imei = imei,
            Serial = serial
        };
    }
}