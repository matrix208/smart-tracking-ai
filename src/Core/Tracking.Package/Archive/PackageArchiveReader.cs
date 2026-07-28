using System.IO.Compression;

namespace Tracking.Package.Archive;

public static class PackageArchiveReader
{
    public static void Extract(
        string packageFile,
        string destination)
    {
        if (!File.Exists(packageFile))
            throw new FileNotFoundException(packageFile);

        if (Directory.Exists(destination))
            Directory.Delete(destination, true);

        Directory.CreateDirectory(destination);

        ZipFile.ExtractToDirectory(
            packageFile,
            destination);
    }
}