using System.IO;

namespace Europa1400.Tools.Decoder.Ed3
{
    public class Transform
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public static Transform FromBytes(BinaryReader br)
        {
            return new Transform
            {
                Position = Vector3.FromBytes(br),
                Rotation = Vector3.FromBytes(br)
            };
        }
    }
}
