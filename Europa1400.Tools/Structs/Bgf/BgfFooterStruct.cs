using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Bgf;

public class BgfFooterStruct
{
    public required BgfTextureNameStruct[] TextureNames { get; init; }

    public static BgfFooterStruct FromBytes(BinaryReader br)
    {
        var textureNames = br.ReadUntilException(BgfTextureNameStruct.FromBytes, typeof(InvalidDataException),
            typeof(EndOfStreamException));

        return new BgfFooterStruct
        {
            TextureNames = textureNames
        };
    }
}