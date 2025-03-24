using System.Collections.Generic;
using System.Linq;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline
{
    public class AssetSelection<TAsset> where TAsset : IGameAsset
    {
        private readonly IEnumerable<TAsset> assets;

        public AssetSelection(IEnumerable<TAsset> assets)
        {
            this.assets = assets;
        }
        
        public IReadOnlyList<TAsset> Assets => assets.ToList();
    }
}