using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Res;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class ResDecoder : IDecoder
    {
        public Task<object> DecodeAsync(GameAsset asset, CancellationToken cancellationToken = default)
        {
            return BinaryAssetDecoder.Decode(asset, ResStruct.FromBytes);
        }
    }
}
