using System.IO;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfPolygonMappingStruct
    {
        public BgfFaceStruct Face { get; private set; }
        public BgfTextureMappingStruct TextureMapping { get; private set; }
        public byte TextureIndex { get; private set; }

        public static BgfPolygonMappingStruct FromBytes(BinaryReader br)
        {
            var face = BgfFaceStruct.FromBytes(br);
            var textureMapping = BgfTextureMappingStruct.FromBytes(br);
            var textureIndex = br.ReadByte();

            return new BgfPolygonMappingStruct
            {
                Face = face,
                TextureMapping = textureMapping,
                TextureIndex = textureIndex
            };
        }
    }
}