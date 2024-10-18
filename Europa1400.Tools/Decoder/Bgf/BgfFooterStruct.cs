using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Bgf;

internal class BgfFooterStruct
{
    internal required BgfTextureNameStruct[] TextureNames { get; init; }

    internal static BgfFooterStruct FromBytes(BinaryReader br)
    {
        var textureNames = br.ReadUntilException(BgfTextureNameStruct.FromBytes, typeof(InvalidDataException), typeof(EndOfStreamException));

        return new BgfFooterStruct
        {
            TextureNames = textureNames
        };
    }
}
