using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline;

public class AssetSelection<TAsset>(IEnumerable<TAsset> assets)
    where TAsset : IGameAsset
{
    public IReadOnlyList<TAsset> Assets { get; } = assets.ToList();
}