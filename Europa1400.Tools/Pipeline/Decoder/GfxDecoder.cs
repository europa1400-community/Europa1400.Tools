using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Gfx;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class GfxDecoder : IDecoder<GfxAsset, GfxStruct>
    {
        public Task<GfxStruct> DecodeAsync(GfxAsset asset, CancellationToken cancellationToken = default)
        {
            using var stream = File.OpenRead(asset.FilePath);
            using var br = new BinaryReader(stream);
            var result = GfxStruct.FromBytes(br);
            return Task.FromResult(result);
        }
    }
}