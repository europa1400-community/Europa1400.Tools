using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Sbf;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class SbfDecoder : IDecoder<SbfAsset, SbfStruct>
    {
        public Task<SbfStruct> DecodeAsync(SbfAsset asset, CancellationToken cancellationToken = default)
        {
            using var stream = File.OpenRead(asset.FilePath);
            using var reader = new BinaryReader(stream);
            return Task.FromResult(SbfStruct.FromBytes(reader));
        }
    }
}