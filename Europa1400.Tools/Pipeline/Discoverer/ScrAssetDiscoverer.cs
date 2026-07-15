using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class ScrAssetDiscoverer : IAssetDiscoverer<ScrAsset>
    {
        public IEnumerable<ScrAsset> DiscoverAllFromGame(string gamePath)
        {
            if (!Directory.Exists(gamePath))
                return Array.Empty<ScrAsset>();

            return Directory.GetFiles(gamePath, "*.scr", SearchOption.AllDirectories)
                .Select(filePath => new ScrAsset(
                    filePath,
                    Path.GetRelativePath(gamePath, filePath).Replace('\\', '/')
                ));
        }

        public ScrAsset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("SCR file not found", filePath);

            return new ScrAsset(Path.GetFullPath(filePath), Path.GetFileName(filePath));
        }
    }
}
