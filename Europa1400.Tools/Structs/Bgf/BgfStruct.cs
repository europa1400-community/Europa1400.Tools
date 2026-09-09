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

        /// <summary>
        /// MappingObject.TextureIndex addresses footer names (body then head, …),
        /// not <see cref="Textures"/> file order (often head then body).
        /// </summary>
        internal int ResolveFileTextureIndex(byte mappingTextureIndex)
        {
            if (mappingTextureIndex == byte.MaxValue) return -1;

            var names = Footer?.TextureNames;
            if (names != null && mappingTextureIndex < names.Length)
            {
                var footerName = names[mappingTextureIndex].ResolvedName.NormalizeName();
                for (var i = 0; i < Textures.Length; i++)
                {
                    if (Textures[i].Name.NormalizeName() == footerName)
                        return i;
                }
            }

            return names == null && mappingTextureIndex < Textures.Length ? mappingTextureIndex : -1;
        }

        public static BgfStruct FromBytes(BinaryReader br)
        {
            var header = BgfHeaderStruct.FromBytes(br);
            var textures = br.ReadUntilException(BgfTextureStruct.FromBytes, typeof(InvalidDataException),
                typeof(EndOfStreamException));
            var gameObjects = br.ReadUntilException(BgfGameObjectStruct.FromBytes, typeof(InvalidDataException),
                typeof(EndOfStreamException));
            var mappingObject = BgfMappingObjectStruct.FromBytes(br);
            var footer = BgfFooterStruct.FromBytes(br, mappingObject.TextureCount);

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