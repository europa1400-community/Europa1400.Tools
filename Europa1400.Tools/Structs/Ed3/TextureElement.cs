using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Ed3
{
    public class TextureElement
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<byte> Values { get; set; }

        public static TextureElement FromBytes(BinaryReader br, int parentSize)
        {
            return new TextureElement
            {
                Name = br.ReadCString(),
                Width = br.ReadInt32(),
                Height = br.ReadInt32(),
                Values = br.ReadBytes(parentSize).ToList()
            };
        }
    }
}
