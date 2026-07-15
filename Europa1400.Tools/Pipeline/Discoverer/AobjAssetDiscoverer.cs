using System;
using System.Collections.Generic;
using System.IO;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    internal class AobjAssetDiscoverer : IAssetDiscoverer<AobjAsset>
    {
        public IEnumerable<AobjAsset> DiscoverAllFromGame(string gamePath)
        {
            var relativePath = Path.Combine("Data", "A_Obj.dat");
            var filePath = Path.Combine(gamePath, relativePath);

            if (!File.Exists(filePath))
                return Array.Empty<AobjAsset>();

            return new List<AobjAsset> { new AobjAsset(filePath, relativePath) };
        }

        public AobjAsset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("AObj file not found", filePath);

            return new AobjAsset(Path.GetFullPath(filePath), Path.GetFileName(filePath));
        }
    }
}