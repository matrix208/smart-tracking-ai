using System.Text.Json;
using Tracking.Package.Models;

namespace Tracking.Package.Manifest;

public static class ManifestSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static PackageManifest Load(string file)
    {
        if (!File.Exists(file))
            throw new FileNotFoundException(file);

        string json = File.ReadAllText(file);

        PackageManifest? manifest =
            JsonSerializer.Deserialize<PackageManifest>(
                json,
                Options);

        if (manifest is null)
            throw new InvalidOperationException(
                "Invalid manifest.");

        return manifest;
    }

    public static void Save(
        string file,
        PackageManifest manifest)
    {
        string json =
            JsonSerializer.Serialize(
                manifest,
                Options);

        File.WriteAllText(file, json);
    }
}