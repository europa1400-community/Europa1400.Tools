using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Ageb;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class AgebDecoder : IDecoder
    {
        public Task<object> DecodeAsync(GameAsset asset, CancellationToken cancellationToken = default)
        {
            using var stream = File.OpenRead(asset.FilePath);
            using var reader = new BinaryReader(stream);
            var result = AgebStruct.FromBytes(reader);
            return Task.FromResult<object>(result);
        }
    }
}