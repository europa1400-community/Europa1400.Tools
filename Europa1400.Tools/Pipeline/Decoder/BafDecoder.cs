using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Decoder.Baf;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class BafDecoder : IDecoder
    {
        public Task<object> DecodeAsync(GameAsset asset, CancellationToken cancellationToken = default)
        {
            using var stream = File.OpenRead(asset.FilePath);
            using var reader = new BinaryReader(stream);
            var result = BafStruct.FromBytes(reader);
            return Task.FromResult<object>(result);
        }
    }
}
