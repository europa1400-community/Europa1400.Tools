using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class OgrAssetDiscoverer : IAssetDiscoverer<OgrAsset>
    {
        public IEnumerable<OgrAsset> DiscoverAllFromGame(string gamePath)
        {
            if (!Directory.Exists(gamePath))
                return Array.Empty<OgrAsset>();

            return Directory.GetFiles(gamePath, "*.ogr", SearchOption.AllDirectories)
                .Select(filePath => new OgrAsset(
                    filePath,
                    Path.GetRelativePath(gamePath, filePath).Replace('\\', '/')
                ));
        }

        public OgrAsset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("OGR file not found", filePath);

            return new OgrAsset(Path.GetFullPath(filePath), Path.GetFileName(filePath));
        }
    }
}
