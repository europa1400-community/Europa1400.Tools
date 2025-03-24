using System.IO;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Bgf;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class BgfDecoder : IDecoder<BgfAsset, BgfStruct>
    {
        public BgfStruct Decode(BgfAsset asset)
        {
            using var stream = File.OpenRead(asset.FilePath);
            using var reader = new BinaryReader(stream);
            return BgfStruct.FromBytes(reader);
        }
    }
}