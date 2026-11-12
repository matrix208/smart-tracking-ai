using System.Security.Cryptography;

namespace Tracking.Package.Security;

public static class SignatureGenerator
{
    public static string SignFile(
        string packageFile,
        string privateKeyPemPath)
    {
        if (!File.Exists(packageFile))
            throw new FileNotFoundException(packageFile);

        if (!File.Exists(privateKeyPemPath))
            throw new FileNotFoundException(privateKeyPemPath);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(privateKeyPemPath));

        byte[] hash = ComputeHash(packageFile);

        byte[] signature = rsa.SignHash(
            hash,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        string signatureFile = packageFile + ".sig";
        string base64Signature = Convert.ToBase64String(signature);

        File.WriteAllText(signatureFile, base64Signature);

        return signatureFile;
    }

    private static byte[] ComputeHash(string file)
    {
        using var stream = File.OpenRead(file);
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(stream);
    }
}
