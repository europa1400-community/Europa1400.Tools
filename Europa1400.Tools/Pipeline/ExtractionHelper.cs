using System.IO.Compression;

namespace Europa1400.Tools.Pipeline;

public static class ExtractionHelper
{
    public static void EnsureExtracted(string archivePath, string targetDirectory)
    {
        if (Directory.Exists(targetDirectory))
            Directory.Delete(targetDirectory, true);

        if (!File.Exists(archivePath))
            throw new FileNotFoundException("Missing archive: " + archivePath);

        Console.WriteLine($"Extracting {Path.GetFileName(archivePath)} to {targetDirectory}...");

        Directory.CreateDirectory(targetDirectory);
        ZipFile.ExtractToDirectory(archivePath, targetDirectory);
    }
}