using System.IO;

namespace Europa1400.Tools.Decoder.Ed3
{
    public class MainCameraNums
    {
        public byte Num1 { get; set; }
        public byte Num2 { get; set; }
        public byte Num3 { get; set; }
        public byte Num4 { get; set; }

        public static MainCameraNums FromBytes(BinaryReader br)
        {
            return new MainCameraNums
            {
                Num1 = br.ReadByte(),
                Num2 = br.ReadByte(),
                Num3 = br.ReadByte(),
                Num4 = br.ReadByte()
            };
        }
    }
}
