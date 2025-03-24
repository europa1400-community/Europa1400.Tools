namespace Europa1400.Tools.Structs.Bgf;

public class BgfTextureMappingStruct
{
    public required Vector3Struct ValuesU { get; init; }
    public required Vector3Struct ValuesV { get; init; }
    public required Vector3Struct ValuesW { get; init; }

    public BgfTextureCoordinatesStruct TextureCoordinates1 => new()
    {
        U = ValuesU.X,
        V = ValuesV.X,
        W = ValuesW.X
    };

    public BgfTextureCoordinatesStruct TextureCoordinates2 => new()
    {
        U = ValuesU.Y,
        V = ValuesV.Y,
        W = ValuesW.Y
    };

    public BgfTextureCoordinatesStruct TextureCoordinates3 => new()
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