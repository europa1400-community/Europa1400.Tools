using System;
using System.IO;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Decoder
{
    internal static class BinaryAssetDecoder
    {
        public static Task<object> Decode(GameAsset asset, Func<BinaryReader, object> fromBytes)
        {
            using var stream = File.OpenRead(asset.FilePath);
            using var reader = new BinaryReader(stream);
            return Task.FromResult(fromBytes(reader));
        }
    }
}
