using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Bgf;

internal class BgfStruct
{
    internal required BgfHeaderStruct Header { get; init; }
    internal required IEnumerable<BgfTextureStruct> Textures { get; init; }
    internal required IEnumerable<BgfGameObjectStruct> GameObjects { get; init; }
    internal required BgfMappingObjectStruct MappingObject { get; init; }
    internal required BgfFooterStruct Footer { get; init; }

    internal static BgfStruct FromBytes(BinaryReader br)
    {
        var header = BgfHeaderStruct.FromBytes(br);
        var textures = br.ReadUntilException(BgfTextureStruct.FromBytes, typeof(InvalidDataException), typeof(EndOfStreamException));
        var gameObjects = br.ReadUntilException(BgfGameObjectStruct.FromBytes, typeof(InvalidDataException), typeof(EndOfStreamException));
        var mappingObject = BgfMappingObjectStruct.FromBytes(br);
        var footer = BgfFooterStruct.FromBytes(br);

        return new BgfStruct
        {
            Header = header,
            Textures = textures,
            GameObjects = gameObjects,
            MappingObject = mappingObject,
            Footer = footer
        };
    }
}
