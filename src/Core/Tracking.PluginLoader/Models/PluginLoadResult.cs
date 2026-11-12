using Tracking.SDK.Interfaces;

namespace Tracking.PluginLoader.Models;

public sealed class PluginLoadResult
{
    public required IProtocolPlugin Plugin { get; init; }

    public required string Folder { get; init; }
}