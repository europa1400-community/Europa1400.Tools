using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class Ed3AssetDiscoverer : IAssetDiscoverer<Ed3Asset>
    {
        public IEnumerable<Ed3Asset> DiscoverAllFromGame(string gamePath)
        {
            if (!Directory.Exists(gamePath))
                return Array.Empty<Ed3Asset>();

            return Directory.GetFiles(gamePath, "*.ed3", SearchOption.AllDirectories)
                .Select(filePath => new Ed3Asset(
                    filePath,
                    Path.GetRelativePath(gamePath, filePath).Replace('\\', '/')
                ));
        }

        public Ed3Asset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("ED3 file not found", filePath);

            return new Ed3Asset(Path.GetFullPath(filePath), Path.GetFileName(filePath));
        }
    }
}
