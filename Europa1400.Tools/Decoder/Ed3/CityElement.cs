using System.Collections.Generic;
using System.IO;

namespace Europa1400.Tools.Decoder.Ed3
{
    public class CityElement
    {
        public List<byte> HeightData1 { get; set; }
        public int? CityElementNum1 { get; set; }
        public byte? CityElementNum2 { get; set; }
        public bool? Flag1 { get; set; }
        public List<byte>? HeightData2 { get; set; }
        public bool HasWaterElement { get; set; }
        public WaterElement? WaterElement { get; set; }
        public TextureElement TextureElement { get; set; }
        public CityFooter Footer { get; set; }

        public static CityElement FromBytes(BinaryReader br, byte grandparentType, int width, int height)
        {
            var size = width * height;
            var element = new CityElement
            {
                HeightData1 = ReadHelper.ReadBytesAsList(br, size)
            };

            if (grandparentType != 0xBA)
            {
                element.CityElementNum1 = br.ReadInt32();
                br.ReadByte(); // zero1
                element.CityElementNum2 = br.ReadByte();
                element.Flag1 = br.ReadBoolean();
                var heightData2Len = size * 4;
                element.HeightData2 = ReadHelper.ReadBytesAsList(br, heightData2Len);
                br.ReadByte(); // zero2
            }

            element.HasWaterElement = br.ReadBoolean();

            if (element.HasWaterElement)
                element.WaterElement = WaterElement.FromBytes(br, width, height);

            element.TextureElement = TextureElement.FromBytes(br, size);
            element.Footer = CityFooter.FromBytes(br);

            return element;
        }
    }
}
