namespace Europa1400.Tools.Decoder.Bgf;

internal class BgfPolygonMappingStruct
{
    internal required BgfFaceStruct Face { get; init; }
    internal required BgfTextureMappingStruct TextureMapping { get; init; }
    internal required byte TextureIndex { get; init; }

    internal static BgfPolygonMappingStruct FromBytes(BinaryReader br)
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
