using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Discoverer;

namespace Europa1400.Tools.Pipeline;

public static class GameAssets
{
    public static GameAssetSelector<TAsset> OfType<TAsset>() where TAsset : IGameAsset
    {
        return new GameAssetSelector<TAsset>();
    }

    public static AssetSelection<TAsset> FromGameInstallation<TAsset>(
        this GameAssetSelector<TAsset> selector, string gamePath)
        where TAsset : IGameAsset
    {
        if (!Path.Exists(gamePath))
            throw new DirectoryNotFoundException($"Game path not found: {gamePath}");
        return new AssetSelection<TAsset>(selector.Discoverer.DiscoverAllFromGame(gamePath));
    }

    public static AssetSelection<BgfAsset> FromGameInstallation(
        this GameAssetSelector<BgfAsset> selector, string gamePath, BgfDiscoveryOptions options)
    {
        if (!Path.Exists(gamePath))
            throw new DirectoryNotFoundException($"Game path not found: {gamePath}");
        if (selector.Discoverer is not BgfAssetDiscoverer discoverer)
            throw new InvalidOperationException("BGF discoverer not registered.");

        var assets = discoverer.DiscoverAllFromGame(gamePath, options);
        return new AssetSelection<BgfAsset>(assets);
    }
}