namespace Europa1400.Tools.Decoder.Structs;

public class BgfPolygonMappingStruct
{
    public required BgfFaceStruct Face { get; init; }
    public required BgfTextureMappingStruct TextureMapping { get; init; }
    public byte TextureIndex { get; init; }

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
