using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public interface IDecoder
    {
        Task<object> DecodeAsync(GameAsset asset, CancellationToken cancellationToken = default);
    }
}