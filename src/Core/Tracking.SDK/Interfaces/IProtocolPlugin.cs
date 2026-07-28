using Tracking.SDK.Metadata;
using Tracking.SDK.Models;

namespace Tracking.SDK.Interfaces;

public interface IProtocolPlugin
{
    PluginManifest Manifest { get; }

    bool CanHandle(ReadOnlySpan<byte> packet);

    ValueTask<DeviceMessage?> DecodeAsync(
        ReadOnlyMemory<byte> packet,
        IDeviceSession session,
        CancellationToken cancellationToken = default);

    ValueTask<ReadOnlyMemory<byte>> EncodeAsync(
        DeviceCommand command,
        CancellationToken cancellationToken = default);
}