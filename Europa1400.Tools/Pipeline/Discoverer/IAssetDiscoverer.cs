using System.Collections.Generic;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public interface IAssetDiscoverer<out TAsset> where TAsset : IGameAsset
    {
        IEnumerable<TAsset> DiscoverAllFromGame(string gamePath);
        TAsset WrapSingleFile(string filePath);
    }
}