namespace Tracking.Plugin.Abstractions.Interfaces;

public interface IPluginInstaller
{
    Task<PluginInstallResult> InstallAsync(
        string packageFilePath,
        CancellationToken cancellationToken = default);

    bool Uninstall(string pluginId);
}

public sealed class PluginInstallResult
{
    public required string PluginId { get; init; }

    public required string Name { get; init; }

    public required string Version { get; init; }
}
