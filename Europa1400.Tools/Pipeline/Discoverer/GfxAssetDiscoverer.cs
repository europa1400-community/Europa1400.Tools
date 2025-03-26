using System.Collections.Generic;
using System.IO;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class GfxAssetDiscoverer : IAssetDiscoverer<GfxAsset>
    {
        public IEnumerable<GfxAsset> DiscoverAllFromGame(string gamePath)
        {
            var gfxDir = Path.Combine(gamePath, "gfx");

            if (!Directory.Exists(gfxDir))
                yield break;

            var filePaths = Directory.GetFiles(gfxDir, "*.gfx", SearchOption.TopDirectoryOnly);
            foreach (var filePath in filePaths)
            {
                var relativePath = Path.GetRelativePath(gfxDir, filePath).Replace('\\', '/');
                yield return new GfxAsset(filePath, relativePath);
            }
        }

        public GfxAsset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("GFX file not found", filePath);

            return new GfxAsset(
                Path.GetFullPath(filePath),
                Path.GetFileName(filePath)
            );
        }
    }
}