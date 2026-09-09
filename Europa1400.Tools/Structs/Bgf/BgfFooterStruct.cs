using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfFooterStruct
    {
        public BgfTextureNameStruct[] TextureNames { get; set; }

        public static BgfFooterStruct FromBytes(BinaryReader br, uint textureCount)
        {
            // The runtime texture table is followed by attachment data, not more names.
            return new BgfFooterStruct
            {
                TextureNames = br.ReadArray(BgfTextureNameStruct.FromBytes, textureCount)
            };
        }
    }
}
