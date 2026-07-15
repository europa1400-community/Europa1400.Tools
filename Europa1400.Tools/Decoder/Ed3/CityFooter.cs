using System.Collections.Generic;
using System.IO;

namespace Europa1400.Tools.Decoder.Ed3
{
    public class CityFooter
    {
        public List<string> Names { get; set; }
        public List<float> Values { get; set; }

        public static CityFooter FromBytes(BinaryReader br)
        {
            var footer = new CityFooter
            {
                Names = new List<string>(8)
            };
            for (int i = 0; i < 8; i++)
                footer.Names.Add(ReadHelper.ReadCString(br));

            footer.Values = ReadHelper.ReadFloats(br, 5);
            return footer;
        }
    }
}
