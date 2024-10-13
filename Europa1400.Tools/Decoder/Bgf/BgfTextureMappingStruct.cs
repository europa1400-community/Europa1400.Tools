namespace Europa1400.Tools.Decoder.Bgf;

internal class BgfTextureMappingStruct
{
    internal required Vector3Struct ValuesU { get; init; }
    internal required Vector3Struct ValuesV { get; init; }
    internal required Vector3Struct ValuesW { get; init; }

    internal BgfTextureCoordinatesStruct TextureCoordinates1 => new()
    {
        U = ValuesU.X,
        V = ValuesV.X,
        W = ValuesW.X
    };

    internal BgfTextureCoordinatesStruct TextureCoordinates2 => new()
    {
        U = ValuesU.Y,
        V = ValuesV.Y,
        W = ValuesW.Y
    };

    internal BgfTextureCoordinatesStruct TextureCoordinates3 => new()
    {
        U = ValuesU.Z,
        V = ValuesV.Z,
        W = ValuesW.Z
    };

    internal static BgfTextureMappingStruct FromBytes(BinaryReader br)
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
