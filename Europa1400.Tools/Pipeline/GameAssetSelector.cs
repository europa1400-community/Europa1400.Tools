using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Discoverer;

namespace Europa1400.Tools.Pipeline
{
    public class GameAssetSelector<TAsset> where TAsset : IGameAsset
    {
        public IAssetDiscoverer<TAsset> Discoverer { get; } = AssetDiscovererRegistry.Get<TAsset>();
    }
}