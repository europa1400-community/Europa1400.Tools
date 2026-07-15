using System.Collections.Generic;
using System.IO;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class AgebAssetDiscoverer : IAssetDiscoverer<AgebAsset>
    {
        public IEnumerable<AgebAsset> DiscoverAllFromGame(string gamePath)
        {
            var relativePath = Path.Combine("Data", "A_Geb.dat");
            var filePath = Path.Combine(gamePath, relativePath);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Ageb file not found", filePath);

            return new List<AgebAsset> { new AgebAsset(filePath, relativePath) };
        }

        public AgebAsset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Ageb file not found", filePath);

            return new AgebAsset(Path.GetFullPath(filePath), Path.GetFileName(filePath));
        }
    }
}