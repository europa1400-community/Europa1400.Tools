using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Structs;

public class BgfPolygonStruct
{
    public required BgfFaceStruct Face { get; init; }
    public required BgfTextureMappingStruct TextureMapping { get; init; }
    public Vector3Struct? Normal { get; init; }
    public byte? TextureIndex { get; init; }

    public static BgfPolygonStruct FromBytes(byte[] data)
    {
        var ms = new MemoryStream(data);
        var br = new BinaryReader(ms);

        return FromBytes(br);
    }

    public static BgfPolygonStruct FromBytes(BinaryReader br)
    {
        var face = BgfFaceStruct.FromBytes(br.ReadBytes(12));
        br.SkipOptionalByte(0x1E);
        var textureMapping = BgfTextureMappingStruct.FromBytes(br.ReadBytes(36));
        var optionalPresent1 = br.SkipOptionalByte(0x1F);
        var normal = optionalPresent1 ? Vector3Struct.FromBytes(br.ReadBytes(12)) : null;
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
