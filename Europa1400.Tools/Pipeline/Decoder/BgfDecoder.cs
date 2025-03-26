using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Bgf;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class BgfDecoder : IDecoder
    {
        public Task<object> DecodeAsync(GameAsset asset, CancellationToken cancellationToken = default)
        {
            using var stream = File.OpenRead(asset.FilePath);
            using var reader = new BinaryReader(stream);
            return Task.FromResult<object>(BgfStruct.FromBytes(reader));
        }
    }
}