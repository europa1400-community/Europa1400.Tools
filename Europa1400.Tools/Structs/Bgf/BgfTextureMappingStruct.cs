using System.IO;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfTextureMappingStruct
    {
        public Vector3Struct ValuesU { get; set; }
        public Vector3Struct ValuesV { get; set; }
        public Vector3Struct ValuesW { get; set; }

        public BgfTextureCoordinatesStruct TextureCoordinates1 => new BgfTextureCoordinatesStruct
        {
            U = ValuesU.X,
            V = ValuesV.X,
            W = ValuesW.X
        };

        public BgfTextureCoordinatesStruct TextureCoordinates2 => new BgfTextureCoordinatesStruct
        {
            U = ValuesU.Y,
            V = ValuesV.Y,
            W = ValuesW.Y
        };

        public BgfTextureCoordinatesStruct TextureCoordinates3 => new BgfTextureCoordinatesStruct
        {
            U = ValuesU.Z,
            V = ValuesV.Z,
            W = ValuesW.Z
        };

        public static BgfTextureMappingStruct FromBytes(BinaryReader br)
        {
            var valuesU = Vector3Struct.FromBytes(br);
            var valuesV = Vector3Struct.FromBytes(br);
            var valuesW = Vector3Struct.FromBytes(br);

            return new BgfTextureMappingStruct
            {
                ValuesU = valuesU,
                ValuesV = valuesV,
                ValuesW = valuesW
            };
        }
    }
}