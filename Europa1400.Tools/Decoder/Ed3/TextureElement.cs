using System.Collections.Generic;
using System.IO;

namespace Europa1400.Tools.Decoder.Ed3
{
    public class TextureElement
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<byte> Values { get; set; }

        public static TextureElement FromBytes(BinaryReader br, int parentSize)
        {
            var element = new TextureElement
            {
                Name = ReadHelper.ReadCString(br),
                Width = br.ReadInt32(),
                Height = br.ReadInt32()
            };

            element.Values = ReadHelper.ReadBytesAsList(br, parentSize);

            return element;
        }
    }
}
