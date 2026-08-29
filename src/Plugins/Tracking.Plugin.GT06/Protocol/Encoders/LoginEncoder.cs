using System.Globalization;
using Tracking.Plugin.GT06.Protocol.Protocols;

namespace Tracking.Plugin.GT06.Protocol.Encoders;

public sealed class LoginEncoder
{
    public ReadOnlyMemory<byte> Encode(
        string imei,
        ushort serial)
    {
        if (string.IsNullOrWhiteSpace(imei))
            throw new ArgumentException("IMEI is required.", nameof(imei));

        if (imei.Length % 2 != 0)
            imei = "0" + imei;

        if (imei.Any(c => !char.IsDigit(c)))
            throw new ArgumentException("IMEI must contain digits only.", nameof(imei));

        var imeiBytes = new byte[imei.Length / 2];

        for (var i = 0; i < imeiBytes.Length; i++)
        {
            imeiBytes[i] = byte.Parse(
                imei.AsSpan(i * 2, 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture);
        }

        return Gt06PacketBuilder.Build(
            Gt06MessageType.Login,
            imeiBytes,
            serial);
    }
}
