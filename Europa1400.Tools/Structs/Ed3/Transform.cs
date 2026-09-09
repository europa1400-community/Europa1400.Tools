using System.IO;
using Europa1400.Tools.Structs;

namespace Europa1400.Tools.Structs.Ed3
{
    public class Transform
    {
        public Vector3Struct Position { get; set; }
        public Vector3Struct Rotation { get; set; }

        public static Transform FromBytes(BinaryReader br)
        {
            return new Transform
            {
                Position = Vector3Struct.FromBytes(br),
                Rotation = Vector3Struct.FromBytes(br)
            };
        }
    }
}
