using System.IO;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfFaceStruct
    {
        public uint A { get; set; }
        public uint B { get; set; }
        public uint C { get; set; }

        public static BgfFaceStruct FromBytes(BinaryReader br)
        {
            var a = br.ReadUInt32();
            var b = br.ReadUInt32();
            var c = br.ReadUInt32();

            return new BgfFaceStruct
            {
                A = a,
                B = b,
                C = c
            };
        }
    }
}