using System.IO.Compression;

namespace Tracking.Package.Archive;

public static class PackageArchiveWriter
{
    public static void Create(
        string sourceDirectory,
        string outputFile)
    {
        if (!Directory.Exists(sourceDirectory))
            throw new DirectoryNotFoundException(sourceDirectory);

        if (File.Exists(outputFile))
            File.Delete(outputFile);

        ZipFile.CreateFromDirectory(
            sourceDirectory,
            outputFile,
            CompressionLevel.Optimal,
            false);
    }
}