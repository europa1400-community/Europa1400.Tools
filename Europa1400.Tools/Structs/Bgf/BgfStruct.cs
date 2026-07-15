using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfStruct
    {
        public BgfHeaderStruct Header { get; set; }
        public BgfTextureStruct[] Textures { get; set; }
        public BgfGameObjectStruct[] GameObjects { get; set; }
        public BgfMappingObjectStruct MappingObject { get; set; }
        public BgfFooterStruct Footer { get; set; }

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
}