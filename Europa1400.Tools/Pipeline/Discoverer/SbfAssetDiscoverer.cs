using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class SbfAssetDiscoverer : IAssetDiscoverer<SbfAsset>
    {
        static SbfAssetDiscoverer()
        {
            AssetDiscovererRegistry.Register(new SbfAssetDiscoverer());
        }

        public IEnumerable<SbfAsset> DiscoverAllFromGame(string gamePath)
        {
            var sfxRoot = Path.Combine(gamePath, "sfx");

            if (!Directory.Exists(sfxRoot))
                return Array.Empty<SbfAsset>();

            return Directory.GetFiles(sfxRoot, "*.sbf", SearchOption.AllDirectories)
                .Select(path => new SbfAsset
                {
                    FilePath = path,
                    RelativePath = Path.GetRelativePath(sfxRoot, path).Replace('\\', '/')
                });
        }

        public SbfAsset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("SBF file not found", filePath);

            return new SbfAsset
            {
                FilePath = Path.GetFullPath(filePath),
                RelativePath = Path.GetFileName(filePath)
            };
        }
    }
}