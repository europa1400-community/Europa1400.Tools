using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class BgfAssetDiscoverer : IAssetDiscoverer<BgfAsset>
    {
        public IEnumerable<BgfAsset> DiscoverAllFromGame(string gamePath)
        {
            return DiscoverAllFromGame(gamePath, BgfDiscoveryOptions.Create());
        }

        public BgfAsset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("BGF file not found", filePath);

            return new BgfAsset(
                Path.GetFullPath(filePath),
                Path.GetFileName(filePath),
                null,
                null,
                null
            );
        }

        public IEnumerable<BgfAsset> DiscoverAllFromGame(string gamePath, BgfDiscoveryOptions options)
        {
            var archivePath = Path.Combine(gamePath, "Resources", "objects.bin");
            var extractedRoot = Path.Combine(PipelineSettings.CacheRoot, "objects");

            ExtractionHelper.EnsureExtracted(archivePath, extractedRoot);

            if (!Directory.Exists(extractedRoot))
                yield break;

            var allFiles = Directory.GetFiles(extractedRoot, "*.*", SearchOption.AllDirectories);
            var bgfFiles = allFiles.Where(f => Path.GetExtension(f).Equals(".bgf", StringComparison.OrdinalIgnoreCase));

            foreach (var bgfFile in bgfFiles)
                yield return new BgfAsset(
                    bgfFile,
                    Path.GetRelativePath(extractedRoot, bgfFile).Replace('\\', '/'),
                    null,
                    null,
                    null
                );
        }
    }
}