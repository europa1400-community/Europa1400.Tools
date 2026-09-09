using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Ed3
{
    public class LightBlock
    {
        public List<float> Values { get; set; }
        public int Num { get; set; }

        public static LightBlock FromBytes(BinaryReader br)
        {
            return new LightBlock
            {
                Values = br.ReadSingles(13).ToList(),
                Num = br.ReadInt32()
            };
        }
    }

    public class LightElement
    {
        public bool Flag1 { get; set; }
        public bool Flag2 { get; set; }
        public bool IsNotRLight { get; set; }
        public List<LightBlock> Blocks { get; set; }

        public static LightElement FromBytes(BinaryReader br, byte parentType)
        {
            var blockCount = (parentType == 0xB9 || parentType == 0xAF) ? 7 : 8;

            var element = new LightElement
            {
                Flag1 = br.ReadBoolean(),
                Flag2 = br.ReadBoolean(),
                IsNotRLight = br.ReadBoolean()
            };

            element.Blocks = new List<LightBlock>(blockCount);
            for (int i = 0; i < blockCount; i++)
                element.Blocks.Add(LightBlock.FromBytes(br));

            return element;
        }
    }
}
