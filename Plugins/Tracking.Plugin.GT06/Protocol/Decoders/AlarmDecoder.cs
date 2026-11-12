using BinaryReader = Tracking.Protocol.Binary.BinaryReader;
using Tracking.Plugin.GT06.Protocol.Models;

namespace Tracking.Plugin.GT06.Protocol.Decoders;

public sealed class AlarmDecoder
{
    public AlarmMessage Decode(BinaryReader reader)
    {
        // GT06 Alarm packet payload:
        // Alarm Code + Language + Serial + CRC
        //
        // هنا نقرأ فقط البيانات المهمة،
        // والباقي يترك للـ Frame/CRC layer

        byte alarmCode = reader.ReadByte();

        return new AlarmMessage
        {
            AlarmCode = alarmCode,
            Timestamp = DateTime.UtcNow
        };
    }
}