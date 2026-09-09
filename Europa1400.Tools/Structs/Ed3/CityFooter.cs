using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Ed3
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
            for (var i = 0; i < 8; i++)
                footer.Names.Add(br.ReadCString());

            footer.Values = br.ReadSingles(5).ToList();
            return footer;
        }
    }
}
