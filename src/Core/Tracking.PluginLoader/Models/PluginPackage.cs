using Tracking.SDK.Metadata;

namespace Tracking.PluginLoader.Models;

public sealed class PluginPackage
{
    public required string Folder { get; init; }

    public required PluginManifest Manifest { get; init; }

    public required string AssemblyPath { get; init; }
}