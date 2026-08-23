namespace Tracking.SDK.Metadata;

public sealed class ProtocolMetadata
{
    public string Name { get; init; } = "";

    public string Author { get; init; } = "";

    public string Company { get; init; } = "";

    public string Version { get; init; } = "";

    public string Description { get; init; } = "";

    public int DefaultPort { get; init; }

    public bool SupportsTcp { get; init; }

    public bool SupportsUdp { get; init; }

    public string[] SupportedModels { get; init; } = [];
}