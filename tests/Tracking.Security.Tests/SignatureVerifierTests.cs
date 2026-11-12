using System.Security.Cryptography;
using System.Text;
using Tracking.Package.Security;
using Xunit;

namespace Tracking.Security.Tests;

public sealed class SignatureVerifierTests
{
    [Fact]
    public void Valid_signature_is_accepted()
    {
        using var fixture = new SignatureFixture();
        fixture.CreateSignature();

        Assert.True(SignatureVerifier.VerifyFile(
            fixture.PackagePath,
            fixture.SignaturePath,
            fixture.PublicKeyPath));
    }

    [Fact]
    public void Missing_signature_is_rejected()
    {
        using var fixture = new SignatureFixture();

        Assert.False(SignatureVerifier.VerifyFile(
            fixture.PackagePath,
            fixture.SignaturePath,
            fixture.PublicKeyPath));
    }

    [Fact]
    public void Modified_package_is_rejected()
    {
        using var fixture = new SignatureFixture();
        fixture.CreateSignature();

        File.WriteAllText(
            fixture.PackagePath,
            "Modified package content");

        Assert.False(SignatureVerifier.VerifyFile(
            fixture.PackagePath,
            fixture.SignaturePath,
            fixture.PublicKeyPath));
    }

    [Fact]
    public void Invalid_base64_signature_is_rejected()
    {
        using var fixture = new SignatureFixture();
        File.WriteAllText(fixture.SignaturePath, "Not a valid Base64 signature!");

        Assert.False(SignatureVerifier.VerifyFile(
            fixture.PackagePath,
            fixture.SignaturePath,
            fixture.PublicKeyPath));
    }

    [Fact]
    public void Missing_public_key_throws_file_not_found()
    {
        using var fixture = new SignatureFixture();

        fixture.CreateSignature();
        Assert.Throws<FileNotFoundException>(() =>
            SignatureVerifier.VerifyFile(
                fixture.PackagePath,
                fixture.SignaturePath,
                fixture.MissingPublicKeyPath));
    }

    [Fact]
    public void Default_overload_uses_adjacent_signature_file()
    {
        using var fixture = new SignatureFixture();
        fixture.CreateSignature();

        Assert.True(SignatureVerifier.VerifyFile(
            fixture.PackagePath,
            fixture.PublicKeyPath));
    }

    private sealed class SignatureFixture : IDisposable
    {
        private readonly RSA _rsa = RSA.Create(2048);
        private readonly string _directory =
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public string PackagePath =>
            Path.Combine(_directory, "plugin.zip");

        public string SignaturePath => PackagePath + ".sig";

        public string PublicKeyPath =>
            Path.Combine(_directory, "public.pem");

        public string MissingPublicKeyPath =>
            Path.Combine(_directory, "missing-public.pem");

        public SignatureFixture()
        {
            Directory.CreateDirectory(_directory);

            File.WriteAllBytes(
                PackagePath,
                Encoding.UTF8.GetBytes("Temporary plugin package"));

            File.WriteAllText(
                PublicKeyPath,
                _rsa.ExportSubjectPublicKeyInfoPem());
        }

        public void CreateSignature()
        {
            byte[] packageBytes = File.ReadAllBytes(PackagePath);

            byte[] signature = _rsa.SignData(
                packageBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            File.WriteAllText(
                SignaturePath,
                Convert.ToBase64String(signature));
        }

        public void Dispose()
        {
            _rsa.Dispose();
            Directory.Delete(_directory, recursive: true);
        }
    }
}
