using System.IO.Compression;

namespace Tracking.Package.Archive;

public static class PackageArchiveReader
{
    private const int MaxEntries = 2000;
    private const long MaxTotalSize = 256L * 1024 * 1024;
    private const long MaxEntrySize = 64L * 1024 * 1024;

    public static void Extract(
        string packageFile,
        string destination)
    {
        if (!File.Exists(packageFile))
            throw new FileNotFoundException(packageFile);

        if (Directory.Exists(destination) || File.Exists(destination))
            throw new IOException(
                "Extraction destination must not already exist.");

        string root = Path.GetFullPath(destination);
        string rootPrefix = root.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        var seenPaths = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        var entries = new List<(ZipArchiveEntry Entry, string RelativePath, bool IsDirectory)>();

        using var archive = ZipFile.OpenRead(packageFile);

        if (archive.Entries.Count == 0 || archive.Entries.Count > MaxEntries)
            throw new InvalidDataException(
                $"Package must contain between 1 and {MaxEntries} entries.");

        long declaredTotalSize = 0;

        foreach (var entry in archive.Entries)
        {
            string name = entry.FullName.Replace('\\', '/');

            if (string.IsNullOrWhiteSpace(name) ||
                name.StartsWith('/') ||
                name.Contains(':') ||
                name.Contains('\0'))
            {
                throw new InvalidDataException(
                    $"Unsafe package entry path: {entry.FullName}");
            }

            bool isDirectory = name.EndsWith('/');
            string relativePath = isDirectory
                ? name[..^1]
                : name;

            string[] segments = relativePath.Split('/');

            if (segments.Length == 0 ||
                segments.Any(segment =>
                    string.IsNullOrWhiteSpace(segment) ||
                    segment == "." ||
                    segment == ".."))
            {
                throw new InvalidDataException(
                    $"Unsafe package entry path: {entry.FullName}");
            }

            int unixType = (entry.ExternalAttributes >> 16) & 0xF000;
            bool isReparsePoint =
                (entry.ExternalAttributes & (int)FileAttributes.ReparsePoint) != 0;

            if (unixType == 0xA000 || isReparsePoint)
            {
                throw new InvalidDataException(
                    $"Symbolic links and reparse points are not allowed: {entry.FullName}");
            }

            if (entry.Length < 0 || entry.Length > MaxEntrySize)
            {
                throw new InvalidDataException(
                    $"Package entry exceeds the allowed size: {entry.FullName}");
            }

            if (isDirectory && entry.Length != 0)
                throw new InvalidDataException(
                    $"Directory entry contains data: {entry.FullName}");

            declaredTotalSize = checked(declaredTotalSize + entry.Length);

            if (declaredTotalSize > MaxTotalSize)
                throw new InvalidDataException(
                    "Package exceeds the maximum uncompressed size.");

            string normalizedKey = string.Join('/', segments);

            if (!seenPaths.Add(normalizedKey))
                throw new InvalidDataException(
                    $"Duplicate package entry: {entry.FullName}");

            string fullPath = Path.GetFullPath(
                Path.Combine(root, Path.Combine(segments)));

            if (!fullPath.StartsWith(rootPrefix, comparison))
                throw new InvalidDataException(
                    $"Package entry escapes extraction directory: {entry.FullName}");

            entries.Add((entry, string.Join(
                Path.DirectorySeparatorChar, segments), isDirectory));
        }

        Directory.CreateDirectory(root);

        long actualTotalSize = 0;

        foreach (var item in entries)
        {
            string targetPath = Path.Combine(root, item.RelativePath);

            if (item.IsDirectory)
            {
                Directory.CreateDirectory(targetPath);
                continue;
            }

            string? parent = Path.GetDirectoryName(targetPath);

            if (parent is not null)
                Directory.CreateDirectory(parent);

            using var input = item.Entry.Open();
            using var output = new FileStream(
                targetPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);

            byte[] buffer = new byte[81920];
            long entryBytes = 0;
            int read;

            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                entryBytes = checked(entryBytes + read);
                actualTotalSize = checked(actualTotalSize + read);

                if (entryBytes > MaxEntrySize ||
                    actualTotalSize > MaxTotalSize ||
                    entryBytes > item.Entry.Length)
                {
                    throw new InvalidDataException(
                        "Extracted package data exceeds the allowed size.");
                }

                output.Write(buffer, 0, read);
            }

            if (entryBytes != item.Entry.Length)
                throw new InvalidDataException(
                    $"Package entry size mismatch: {item.Entry.FullName}");
        }
    }
}
