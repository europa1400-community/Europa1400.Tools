using System.IO;

namespace Europa1400.Tools.Structs
{
    public class Vector3Struct
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static Vector3Struct FromBytes(BinaryReader br)
        {
            var x = br.ReadSingle();
            var y = br.ReadSingle();
            var z = br.ReadSingle();

            return new Vector3Struct
            {
                X = x,
                Y = y,
                Z = z
            };
        }
    }
}