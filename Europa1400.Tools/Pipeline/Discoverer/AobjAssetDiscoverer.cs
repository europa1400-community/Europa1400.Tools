using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer;

public class AObjAssetDiscoverer : IAssetDiscoverer<AobjAsset>
{
    static AObjAssetDiscoverer()
    {
        AssetDiscovererRegistry.Register(new AObjAssetDiscoverer());
    }

    public IEnumerable<AobjAsset> DiscoverAllFromGame(string gamePath)
    {
        var file = Path.Combine(gamePath, "Data", "A_Obj.dat");

        if (!File.Exists(file))
            return [];

        return [new AobjAsset { FilePath = file }];
    }

    public AobjAsset WrapSingleFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("AObj file not found", filePath);

        return new AobjAsset { FilePath = Path.GetFullPath(filePath) };
    }
}