using System.Text.Json;
using Tracking.PluginManager.Services;
using Xunit;

namespace Tracking.Security.Tests;

public sealed class InstalledPluginStoreTests
{
    [Fact]
    public void Upgrade_preserves_disabled_state()
    {
        using var fixture = new StoreFixture();

        var first = fixture.CreatePluginSource(
            "1.0.0",
            "Version 1");

        fixture.Store.InstallFromDirectory(
            first,
            "test-plugin");

        Assert.True(
            fixture.Store.SetEnabled("test-plugin", false));

        var second = fixture.CreatePluginSource(
            "2.0.0",
            "Version 2");

        fixture.Store.InstallFromDirectory(
            second,
            "test-plugin");

        var installed = fixture.Store.Get("test-plugin");

        Assert.NotNull(installed);
        Assert.Equal("2.0.0", installed.Version);
        Assert.False(installed.Enabled);

        var assemblyPath = Path.Combine(
            installed.Directory,
            "Test.Plugin.dll");

        Assert.Equal(
            "Version 2",
            File.ReadAllText(assemblyPath));
    }

    [Fact]
    public void Failed_upgrade_preserves_previous_installation()
    {
        using var fixture = new StoreFixture();

        var first = fixture.CreatePluginSource(
            "1.0.0",
            "Original assembly");

        fixture.Store.InstallFromDirectory(
            first,
            "test-plugin");

        Assert.True(
            fixture.Store.SetEnabled("test-plugin", false));

        var previous = fixture.Store.Get("test-plugin");

        Assert.NotNull(previous);

        var previousDirectory = previous.Directory;

        var second = fixture.CreatePluginSource(
            "2.0.0",
            "New assembly",
            invalidManifest: true);

        Assert.Throws<JsonException>(
            () => fixture.Store.InstallFromDirectory(
                second,
                "test-plugin"));

        var installed = fixture.Store.Get("test-plugin");

        Assert.NotNull(installed);
        Assert.Equal("1.0.0", installed.Version);
        Assert.False(installed.Enabled);
        Assert.Equal(previousDirectory, installed.Directory);

        Assert.Equal(
            "Original assembly",
            File.ReadAllText(
                Path.Combine(
                    installed.Directory,
                    "Test.Plugin.dll")));
    }

    private sealed class StoreFixture : IDisposable
    {
        private readonly string _root;

        private readonly string _sources;

        public InstalledPluginStore Store { get; }

        public StoreFixture()
        {
            _root = Path.Combine(
                Path.GetTempPath(),
                "plugin-store-tests-" + Guid.NewGuid().ToString("N"));

            _sources = Path.Combine(_root, "sources");

            Directory.CreateDirectory(_sources);

            Store = new InstalledPluginStore(
                Path.Combine(_root, "installed"));
        }

        public string CreatePluginSource(
            string version,
            string assemblyContent,
            bool invalidManifest = false)
        {
            var source = Path.Combine(
                _sources,
                Guid.NewGuid().ToString("N"));

            var manifestDirectory = Path.Combine(
                source,
                "Manifest");

            Directory.CreateDirectory(manifestDirectory);

            var manifestPath = Path.Combine(
                manifestDirectory,
                "manifest.json");

            if (invalidManifest)
            {
                File.WriteAllText(
                    manifestPath,
                    "{ invalid json");
            }
            else
            {
                var manifest = new InstalledPluginManifest
                {
                    Id = "test-plugin",
                    Name = "Test Plugin",
                    Version = version,
                    Assembly = "Test.Plugin.dll",
                    EntryPoint = "Test.Plugin.EntryPoint",
                    Enabled = true
                };

                File.WriteAllText(
                    manifestPath,
                    JsonSerializer.Serialize(manifest));
            }

            File.WriteAllText(
                Path.Combine(source, "Test.Plugin.dll"),
                assemblyContent);

            return source;
        }

        public void Dispose()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(
                    _root,
                    recursive: true);
            }
        }
    }
}
