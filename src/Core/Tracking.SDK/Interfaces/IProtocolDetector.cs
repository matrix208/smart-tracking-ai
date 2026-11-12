
namespace Tracking.SDK.Interfaces;

public interface IProtocolDetector
{
    bool CanHandle(ReadOnlySpan<byte> packet);
}