using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class TxsAssetDiscoverer : IAssetDiscoverer<TxsAsset>
    {
        public IEnumerable<TxsAsset> DiscoverAllFromGame(string gamePath)
        {
            if (!Directory.Exists(gamePath))
                return Array.Empty<TxsAsset>();

            return Directory.GetFiles(gamePath, "*.txs", SearchOption.AllDirectories)
                .Select(filePath => new TxsAsset(
                    filePath,
                    Path.GetRelativePath(gamePath, filePath).Replace('\\', '/')
                ));
        }

        public TxsAsset WrapSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("TXS file not found", filePath);

            return new TxsAsset(Path.GetFullPath(filePath), Path.GetFileName(filePath));
        }
    }
}
