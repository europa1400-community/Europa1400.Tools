using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer;

public class GfxAssetDiscoverer : IAssetDiscoverer<GfxAsset>
{
    public IEnumerable<GfxAsset> DiscoverAllFromGame(string gamePath)
    {
        var gfxDir = Path.Combine(gamePath, "gfx");

        if (!Directory.Exists(gfxDir))
            yield break;

        var files = Directory.GetFiles(gfxDir, "*.gfx", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
            yield return new GfxAsset
            {
                FilePath = file
            };
    }

    public GfxAsset WrapSingleFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("GFX file not found", filePath);

        return new GfxAsset
        {
            FilePath = Path.GetFullPath(filePath)
        };
    }
}