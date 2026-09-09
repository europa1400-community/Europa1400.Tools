using System;
using System.IO;
using System.IO.Compression;

namespace Europa1400.Tools.Pipeline
{
    public static class ExtractionHelper
    {
        public static void EnsureExtracted(string archivePath, string targetDirectory)
        {
            if (!File.Exists(archivePath))
                throw new FileNotFoundException("Missing archive: " + archivePath);

            var stamp = BuildStamp(archivePath);
            var stampPath = Path.Combine(targetDirectory, ".extract-stamp");

            if (Directory.Exists(targetDirectory) && File.Exists(stampPath) &&
                File.ReadAllText(stampPath) == stamp)
                return;

            if (Directory.Exists(targetDirectory))
                Directory.Delete(targetDirectory, true);

            Console.WriteLine($"Extracting {Path.GetFileName(archivePath)} to {targetDirectory}...");

            Directory.CreateDirectory(targetDirectory);
            ZipFile.ExtractToDirectory(archivePath, targetDirectory);
            File.WriteAllText(stampPath, stamp);
        }

        private static string BuildStamp(string archivePath)
        {
            var info = new FileInfo(archivePath);
            return $"{info.LastWriteTimeUtc.Ticks}:{info.Length}";
        }
    }
}
