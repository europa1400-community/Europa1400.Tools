using System.IO;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Sbf;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class SbfDecoder : IDecoder<SbfAsset, SbfStruct>
    {
        public SbfStruct Decode(SbfAsset asset)
        {
            using var stream = File.OpenRead(asset.FilePath);
            using var reader = new BinaryReader(stream);
            return SbfStruct.FromBytes(reader);
        }
    }
}