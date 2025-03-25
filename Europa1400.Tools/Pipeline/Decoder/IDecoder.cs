using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public interface IDecoder<in TAsset, TDecoded>
        where TAsset : IGameAsset
    {
        Task<TDecoded> DecodeAsync(TAsset asset, CancellationToken cancellationToken = default);
    }
}