using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Bgf;

public class BgfStruct
{
    public required BgfHeaderStruct Header { get; init; }
    public required BgfTextureStruct[] Textures { get; init; }
    public required BgfGameObjectStruct[] GameObjects { get; init; }
    public required BgfMappingObjectStruct MappingObject { get; init; }
    public required BgfFooterStruct Footer { get; init; }

    public static BgfStruct FromBytes(BinaryReader br)
    {
        var header = BgfHeaderStruct.FromBytes(br);
        var textures = br.ReadUntilException(BgfTextureStruct.FromBytes, typeof(InvalidDataException),
            typeof(EndOfStreamException));
        var gameObjects = br.ReadUntilException(BgfGameObjectStruct.FromBytes, typeof(InvalidDataException),
            typeof(EndOfStreamException));
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