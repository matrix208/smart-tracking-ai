namespace Tracking.Plugin.GT06.Protocol.Models;

public sealed class LoginMessage
{
    public string Imei { get; init; } = string.Empty;

    public ushort Serial { get; init; }
}