using Tracking.SDK.Interfaces;

namespace Tracking.PluginLoader.Abstractions;

public interface IPluginLoader
{
    Task<IReadOnlyList<IProtocolPlugin>> LoadAsync(
        string pluginsPath,
        CancellationToken cancellationToken = default);
}