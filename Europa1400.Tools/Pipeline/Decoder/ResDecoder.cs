using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Decoder.Res;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class ResDecoder : IDecoder
    {
        public Task<object> DecodeAsync(GameAsset asset, CancellationToken cancellationToken = default)
        {
            using var stream = File.OpenRead(asset.FilePath);
            using var reader = new BinaryReader(stream);
            return Task.FromResult<object>(ResStruct.FromBytes(reader));
        }
    }
}
