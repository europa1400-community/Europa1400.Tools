namespace Europa1400.Tools.Decoder.Structs;

public class BgfTextureMappingStruct
{
    public required Vector3Struct ValuesU { get; init; }
    public required Vector3Struct ValuesV { get; init; }
    public required Vector3Struct ValuesW { get; init; }

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

    public static BgfTextureMappingStruct FromBytes(byte[] data)
    {
        var ms = new MemoryStream(data);
        var br = new BinaryReader(ms);

        return FromBytes(br);
    }

    public static BgfTextureMappingStruct FromBytes(BinaryReader br)
    {
        var valuesU = Vector3Struct.FromBytes(br.ReadBytes(12));
        var valuesV = Vector3Struct.FromBytes(br.ReadBytes(12));
        var valuesW = Vector3Struct.FromBytes(br.ReadBytes(12));

        return new BgfTextureMappingStruct
        {
            ValuesU = valuesU,
            ValuesV = valuesV,
            ValuesW = valuesW
        };
    }
}
