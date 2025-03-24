using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Discoverer;

namespace Europa1400.Tools.Pipeline;

public static class AssetDiscovererRegistry
{
    private static readonly Dictionary<Type, object> Discoverers = new();

    static AssetDiscovererRegistry()
    {
        Register(new AgebAssetDiscoverer());
        Register(new AObjAssetDiscoverer());
        Register(new SbfAssetDiscoverer());
        Register(new BgfAssetDiscoverer());
        Register(new GfxAssetDiscoverer());
    }

    public static void Register<TAsset>(IAssetDiscoverer<TAsset> discoverer) where TAsset : IGameAsset
    {
        Discoverers[typeof(TAsset)] = discoverer;
    }

    public static IAssetDiscoverer<TAsset> Get<TAsset>() where TAsset : IGameAsset
    {
        if (Discoverers.TryGetValue(typeof(TAsset), out var d))
            return (IAssetDiscoverer<TAsset>)d;

        throw new InvalidOperationException($"No discoverer registered for asset type {typeof(TAsset).Name}");
    }
}