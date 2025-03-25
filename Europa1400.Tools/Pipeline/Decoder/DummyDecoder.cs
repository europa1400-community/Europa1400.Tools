using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Decoder
{
    internal class DummyDecoder : IDecoder<DummyAsset, object>
    {
        public async Task<object> DecodeAsync(DummyAsset asset, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            return new object();
        }
    }
}