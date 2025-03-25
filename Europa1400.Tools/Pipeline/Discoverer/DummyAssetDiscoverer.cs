using System.Collections.Generic;
using System.Linq;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    internal class DummyAssetDiscoverer : IAssetDiscoverer<DummyAsset>
    {
        public IEnumerable<DummyAsset> DiscoverAllFromGame(string gamePath)
        {
            return Enumerable.Range(0, 100).Select(i => new DummyAsset());
        }

        public DummyAsset WrapSingleFile(string filePath)
        {
            return new DummyAsset();
        }
    }
}