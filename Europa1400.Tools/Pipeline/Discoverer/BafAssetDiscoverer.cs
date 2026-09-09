using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class BafAssetDiscoverer : IAssetDiscoverer<BafAsset>
    {
        public IEnumerable<BafAsset> DiscoverAllFromGame(string gamePath)
        {
            var archivePath = Path.Combine(gamePath, "Resources", "animations.bin");
            var extractedRoot = Path.Combine(PipelineSettings.CacheRoot, "animations");

            ExtractionHelper.EnsureExtracted(archivePath, extractedRoot);

            if (!Directory.Exists(extractedRoot))
                yield break;

            var bafFiles = Directory.GetFiles(extractedRoot, "*.baf", SearchOption.AllDirectories);

            foreach (var bafFile in bafFiles)
            {
                var bafRelativePath = Path.GetRelativePath(extractedRoot, bafFile).Replace('\\', '/');
                var bafDirectory = Path.GetDirectoryName(bafFile) ?? string.Empty;
                var bafName = Path.GetFileNameWithoutExtension(bafFile);
                var iniPath = Path.Combine(bafDirectory, bafName + ".ini");
                BafIniAsset? bafIni = null;

                if (File.Exists(iniPath))
                    bafIni = new BafIniAsset(iniPath, Path.GetRelativePath(extractedRoot, iniPath).Replace('\\', '/'));

                yield return new BafAsset(bafFile, bafRelativePath, bafIni);
            }
        }

        public BafAsset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("BAF file not found", filePath);

            var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
            var name = Path.GetFileNameWithoutExtension(filePath);
            var iniPath = Path.Combine(directory, name + ".ini");
            BafIniAsset? bafIni = null;

            if (File.Exists(iniPath))
                bafIni = new BafIniAsset(iniPath, Path.GetFileName(iniPath));

            return new BafAsset(Path.GetFullPath(filePath), Path.GetFileName(filePath), bafIni);
        }
    }
}
