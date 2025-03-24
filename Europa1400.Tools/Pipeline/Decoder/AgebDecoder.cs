using System.IO;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Ageb;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class AgebDecoder : IDecoder<AgebAsset, AgebStruct>
    {
        public AgebStruct Decode(AgebAsset asset)
        {
            using var stream = File.OpenRead(asset.FilePath);
            using var reader = new BinaryReader(stream);
            return AgebStruct.FromBytes(reader);
        }
    }
}