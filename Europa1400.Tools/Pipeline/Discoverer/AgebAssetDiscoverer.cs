using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer;

public class AgebAssetDiscoverer : IAssetDiscoverer<AgebAsset>
{
    public IEnumerable<AgebAsset> DiscoverAllFromGame(string gamePath)
    {
        var filePath = Path.Combine(gamePath, "Data", "A_Geb.dat");

        if (!File.Exists(filePath)) throw new FileNotFoundException("Ageb file not found", filePath);

        return [new AgebAsset { FilePath = filePath }];
    }

    public AgebAsset WrapSingleFile(string filePath)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException("Ageb file not found", filePath);

        return new AgebAsset { FilePath = filePath };
    }
}