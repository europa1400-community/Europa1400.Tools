using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Ed3;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class Ed3Decoder : IDecoder
    {
        public Task<object> DecodeAsync(GameAsset asset, CancellationToken cancellationToken = default)
        {
            return BinaryAssetDecoder.Decode(asset, Ed3Struct.FromBytes);
        }
    }
}
