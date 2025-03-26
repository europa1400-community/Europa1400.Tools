using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfPolygonStruct
    {
        public BgfFaceStruct Face { get; set; }
        public BgfTextureMappingStruct TextureMapping { get; set; }
        public Vector3Struct? Normal { get; set; }
        public byte? TextureIndex { get; set; }

        public static BgfPolygonStruct FromBytes(BinaryReader br)
        {
            var face = BgfFaceStruct.FromBytes(br);
            br.SkipOptionalByte(0x1E);
            var textureMapping = BgfTextureMappingStruct.FromBytes(br);
            var optionalPresent1 = br.SkipOptionalByte(0x1F);
            var normal = optionalPresent1 ? Vector3Struct.FromBytes(br) : null;
            var optionalPresent2 = br.SkipOptionalByte(0x20);
            var textureIndex = optionalPresent2 ? br.ReadByte() as byte? : null;
            br.SkipOptionalByte(0x1D);

            return new BgfPolygonStruct
            {
                Face = face,
                TextureMapping = textureMapping,
                Normal = normal,
                TextureIndex = textureIndex
            };
        }
    }
}