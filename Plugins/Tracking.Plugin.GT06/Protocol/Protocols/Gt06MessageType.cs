namespace Tracking.Plugin.GT06.Protocol.Protocols;

public enum Gt06MessageType : byte
{
    Login              = 0x01,
    GPS                = 0x12,
    Status             = 0x13,
    Alarm              = 0x16,
    String             = 0x15,
    Heartbeat          = 0x23,
    Command            = 0x80,
    Information        = 0x94,
    TimeSync           = 0x8A,
    Unknown            = 0xFF
}