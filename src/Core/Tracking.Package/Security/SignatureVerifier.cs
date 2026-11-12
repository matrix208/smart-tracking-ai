using System.Security.Cryptography;

namespace Tracking.Package.Security;

public static class SignatureVerifier
{
    public static bool VerifyFile(
        string packageFile,
        string signatureFile,
        string publicKeyPemPath)
    {
        if (!File.Exists(packageFile))
            throw new FileNotFoundException(packageFile);

        if (!File.Exists(signatureFile))
            return false;

        if (!File.Exists(publicKeyPemPath))
            throw new FileNotFoundException(publicKeyPemPath);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(publicKeyPemPath));

        byte[] hash = ComputeHash(packageFile);

        byte[] signature;

        try
        {
            signature = Convert.FromBase64String(
                File.ReadAllText(signatureFile).Trim());
        }
        catch (FormatException)
        {
            return false;
        }

        return rsa.VerifyHash(
            hash,
            signature,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
    }

    public static bool VerifyFile(
        string packageFile,
        string publicKeyPemPath)
    {
        return VerifyFile(
            packageFile,
            packageFile + ".sig",
            publicKeyPemPath);
    }

    private static byte[] ComputeHash(string file)
    {
        using var stream = File.OpenRead(file);
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(stream);
    }
}
