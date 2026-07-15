using System.IO;

namespace Europa1400.Tools.Decoder.Ed3
{
    public class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static Vector3 FromBytes(BinaryReader br)
        {
            return new Vector3
            {
                X = br.ReadSingle(),
                Y = br.ReadSingle(),
                Z = br.ReadSingle()
            };
        }
    }
}
