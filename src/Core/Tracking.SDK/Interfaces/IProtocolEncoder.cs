using Tracking.SDK.Models;
using Tracking.SDK.Metadata;
namespace Tracking.SDK.Interfaces;

public interface IProtocolEncoder
{
    ValueTask<ReadOnlyMemory<byte>> EncodeAsync(
        DeviceCommand command,
        CancellationToken cancellationToken = default);
}