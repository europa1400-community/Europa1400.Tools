using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Bgf;

internal class BgfTextureNameStruct
{
    internal required string Name { get; init; }

    internal static BgfTextureNameStruct FromBytes(BinaryReader br)
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
