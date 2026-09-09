using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Baf;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class BafDecoder : IDecoder
    {
        public Task<object> DecodeAsync(GameAsset asset, CancellationToken cancellationToken = default)
        {
            return BinaryAssetDecoder.Decode(asset, BafStruct.FromBytes);
        }
    }
}
