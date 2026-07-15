using System.Collections.Generic;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public interface ISubAssetProvider
    {
        IEnumerable<GameAsset> GetSubAssets(GameAsset asset, object decoded);
    }
}