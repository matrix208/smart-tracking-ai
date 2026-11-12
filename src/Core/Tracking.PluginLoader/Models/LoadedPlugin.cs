using System.Reflection;
using Tracking.SDK.Interfaces;

namespace Tracking.PluginLoader.Models;

public sealed class LoadedPlugin
{
    public required Assembly Assembly { get; init; }

    public required IProtocolPlugin Plugin { get; init; }

    public string Id => Plugin.Manifest.Id;

    public string Name => Plugin.Manifest.Name;

    public string Version => Plugin.Manifest.Version;
}