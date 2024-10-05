namespace Europa1400.Tools.Decoder.Structs;

public class BgfFooterStruct
{
    public required IEnumerable<BgfTextureNameStruct> TextureNames { get; init; }

    public static BgfFooterStruct FromBytes(BinaryReader br)
    {
        var textureNames = br.ReadUntilException(BgfTextureNameStruct.FromBytes, typeof(InvalidDataException), typeof(EndOfStreamException));

        return new BgfFooterStruct
        {
            TextureNames = textureNames
        };
    }
}
