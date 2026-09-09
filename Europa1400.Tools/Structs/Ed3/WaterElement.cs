using System.Collections.Generic;
using System.IO;
using System.Linq;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Ed3
{
    public class WaterData
    {
        public string Name { get; set; }
        public int WaterDataNum1 { get; set; }
        public List<float> Values { get; set; }
        public int WaterDataNum2 { get; set; }

        public static WaterData FromBytes(BinaryReader br)
        {
            var data = new WaterData
            {
                Name = br.ReadCString()
            };
            br.ReadBytes(3);
            data.WaterDataNum1 = br.ReadInt32();
            data.Values = br.ReadSingles(11).ToList();
            data.WaterDataNum2 = br.ReadInt32();
            return data;
        }
    }

    public class WaterElement
    {
        public int WaterDataCount { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<byte> Data { get; set; }
        public List<WaterData> WaterDatas { get; set; }

        public static WaterElement FromBytes(BinaryReader br, int parentWidth, int parentHeight)
        {
            var element = new WaterElement
            {
                WaterDataCount = br.ReadInt32(),
                Width = br.ReadInt32(),
                Height = br.ReadInt32()
            };

            var size = parentWidth * parentHeight;
            element.Data = br.ReadBytes(size).ToList();

            element.WaterDatas = new List<WaterData>(element.WaterDataCount);
            for (var i = 0; i < element.WaterDataCount; i++)
                element.WaterDatas.Add(WaterData.FromBytes(br));

            return element;
        }
    }
}
