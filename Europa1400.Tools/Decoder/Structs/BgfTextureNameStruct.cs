namespace Europa1400.Tools.Decoder.Structs;

public class BgfTextureNameStruct
{
    public required string Name { get; init; }

    public static BgfTextureNameStruct FromBytes(BinaryReader br)
    {
        var name = br.ReadCString();
        br.SkipNonLatin1();
        br.SkipOptionalByte(0x2F);

        return new BgfTextureNameStruct
        {
            Name = name
        };
    }
}
