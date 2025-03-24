using System.IO;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Gfx;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public class GfxDecoder : IDecoder<GfxAsset, GfxStruct>
    {
        public GfxStruct Decode(GfxAsset asset)
        {
            using var br = new BinaryReader(File.OpenRead(asset.FilePath));
            return GfxStruct.FromBytes(br);
        }
    }
}