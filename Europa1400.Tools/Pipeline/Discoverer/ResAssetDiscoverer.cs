using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class ResAssetDiscoverer : IAssetDiscoverer<ResAsset>
    {
        public IEnumerable<ResAsset> DiscoverAllFromGame(string gamePath)
        {
            if (!Directory.Exists(gamePath))
                return Array.Empty<ResAsset>();

            return Directory.GetFiles(gamePath, "*.res", SearchOption.AllDirectories)
                .Select(filePath => new ResAsset(
                    filePath,
                    Path.GetRelativePath(gamePath, filePath).Replace('\\', '/')
                ));
        }

        public ResAsset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("RES file not found", filePath);

            return new ResAsset(Path.GetFullPath(filePath), Path.GetFileName(filePath));
        }
    }
}
