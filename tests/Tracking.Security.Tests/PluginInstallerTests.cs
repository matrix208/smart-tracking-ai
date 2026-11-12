using System.IO.Compression;
using System.Security.Cryptography;
using Tracking.Package.Manifest;
using Tracking.Package.Models;
using Tracking.Plugin.Runtime.Installer;
using Tracking.PluginManager.Services;
using Xunit;

namespace Tracking.Security.Tests;

public sealed class PluginInstallerTests
{
    [Fact]
    public async Task Missing_signature_is_rejected()
    {
        using var fixture = new PluginInstallerFixture();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.Installer.InstallAsync(fixture.PackagePath));

        Assert.False(fixture.Store.IsInstalled("test-plugin"));
    }

    [Fact]
    public async Task Missing_public_key_is_rejected()
    {
        using var fixture = new PluginInstallerFixture();

        fixture.CreateSignature();
        File.Delete(fixture.PublicKeyPath);

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => fixture.Installer.InstallAsync(fixture.PackagePath));

        Assert.False(fixture.Store.IsInstalled("test-plugin"));
    }

    [Fact]
    public async Task Invalid_signature_is_rejected()
    {
        using var fixture = new PluginInstallerFixture();

        File.WriteAllText(
            fixture.PackagePath + ".sig",
            Convert.ToBase64String([1, 2, 3]));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.Installer.InstallAsync(fixture.PackagePath));

        Assert.False(fixture.Store.IsInstalled("test-plugin"));
    }

    [Fact]
    public async Task Valid_signed_package_is_installed()
    {
        using var fixture = new PluginInstallerFixture();

        fixture.CreateSignature();

        var result = await fixture.Installer.InstallAsync(
            fixture.PackagePath);

        Assert.Equal("test-plugin", result.PluginId);
        Assert.Equal("Test Plugin", result.Name);
        Assert.Equal("1.0.0", result.Version);

        Assert.True(fixture.Store.IsInstalled("test-plugin"));

        var installed = fixture.Store.Get("test-plugin");

        Assert.NotNull(installed);
        Assert.Equal("Test Plugin", installed.Name);
        Assert.Equal("1.0.0", installed.Version);
        Assert.True(File.Exists(installed.ManifestPath));
    }

    private sealed class PluginInstallerFixture : IDisposable
    {
        private readonly RSA _rsa;

        private readonly string _root;

        public string PackagePath { get; }

        public string PublicKeyPath { get; }

        public InstalledPluginStore Store { get; }

        public PluginInstaller Installer { get; }

        public PluginInstallerFixture()
        {
            _root = Path.Combine(
                Path.GetTempPath(),
                "plugin-installer-tests-" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(_root);

            _rsa = RSA.Create(2048);

            PublicKeyPath = Path.Combine(
                _root,
                "public-key.pem");

            File.WriteAllText(
                PublicKeyPath,
                _rsa.ExportSubjectPublicKeyInfoPem());

            PackagePath = Path.Combine(
                _root,
                "test-plugin.zip");

            Store = new InstalledPluginStore(
                Path.Combine(_root, "installed"));

            Installer = new PluginInstaller(
                Store,
                PublicKeyPath);

            CreatePackage();
        }

        private void CreatePackage()
        {
            var stagingDirectory = Path.Combine(
                _root,
                "package-content");

            var manifestDirectory = Path.Combine(
                stagingDirectory,
                "Manifest");

            Directory.CreateDirectory(manifestDirectory);

            var manifest = new PackageManifest
            {
                PackageId = "test-plugin",
                DisplayName = "Test Plugin",
                Version = "1.0.0",
                Type = PackageType.Protocol,
                Assembly = "Test.Plugin.dll",
                EntryPoint = "Test.Plugin.EntryPoint",
                Permissions = [],
                Dependencies = []
            };

            ManifestSerializer.Save(
                Path.Combine(manifestDirectory, "manifest.json"),
                manifest);

            File.WriteAllText(
                Path.Combine(stagingDirectory, "Test.Plugin.dll"),
                "Test assembly placeholder");

            ZipFile.CreateFromDirectory(
                stagingDirectory,
                PackagePath);
        }

        public void CreateSignature()
        {
            byte[] packageBytes = File.ReadAllBytes(PackagePath);

            byte[] signature = _rsa.SignData(
                packageBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            File.WriteAllText(
                PackagePath + ".sig",
                Convert.ToBase64String(signature));
        }

        public void Dispose()
        {
            _rsa.Dispose();

            if (Directory.Exists(_root))
            {
                Directory.Delete(
                    _root,
                    recursive: true);
            }
        }
    }
}
