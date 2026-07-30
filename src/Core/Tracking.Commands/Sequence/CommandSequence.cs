using System.Threading;

namespace Tracking.Commands.Sequence;

public sealed class CommandSequence
{
    private int _value;

    public uint Next()
    {
        return (uint)Interlocked.Increment(ref _value);
    }
}
