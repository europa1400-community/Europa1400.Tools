using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class SavAssetDiscoverer : IAssetDiscoverer<SavAsset>
    {
        public IEnumerable<SavAsset> DiscoverAllFromGame(string gamePath)
        {
            if (!Directory.Exists(gamePath))
                return Array.Empty<SavAsset>();

            return Directory.GetFiles(gamePath, "*.sav", SearchOption.AllDirectories)
                .Select(filePath => new SavAsset(
                    filePath,
                    Path.GetRelativePath(gamePath, filePath).Replace('\\', '/')
                ));
        }

        public SavAsset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("SAV file not found", filePath);

            return new SavAsset(Path.GetFullPath(filePath), Path.GetFileName(filePath));
        }
    }
}
