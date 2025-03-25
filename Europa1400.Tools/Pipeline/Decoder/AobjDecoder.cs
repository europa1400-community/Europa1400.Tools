using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Aobj;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class AobjDecoder : IDecoder<AobjAsset, AobjStruct>
    {
        public Task<AobjStruct> DecodeAsync(AobjAsset asset, CancellationToken cancellationToken = default)
        {
            using var stream = File.OpenRead(asset.FilePath);
            using var reader = new BinaryReader(stream);
            return Task.FromResult(AobjStruct.FromBytes(reader));
        }
    }
}