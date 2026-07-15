using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class BafIniAssetDiscoverer : IAssetDiscoverer<BafIniAsset>
    {
        public IEnumerable<BafIniAsset> DiscoverAllFromGame(string gamePath)
        {
            if (!Directory.Exists(gamePath))
                return Array.Empty<BafIniAsset>();

            return Directory.GetFiles(gamePath, "*.bafini", SearchOption.AllDirectories)
                .Select(filePath => new BafIniAsset(
                    filePath,
                    Path.GetRelativePath(gamePath, filePath).Replace('\\', '/')
                ));
        }

        public BafIniAsset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("BAFINI file not found", filePath);

            return new BafIniAsset(Path.GetFullPath(filePath), Path.GetFileName(filePath));
        }
    }
}
