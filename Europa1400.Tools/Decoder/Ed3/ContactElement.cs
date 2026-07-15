using System.Collections.Generic;
using System.IO;

namespace Europa1400.Tools.Decoder.Ed3
{
    public class ContactElement
    {
        public bool Flag1 { get; set; }
        public bool Flag2 { get; set; }
        public List<Vector3> Vectors { get; set; }
        public float Value { get; set; }

        public static ContactElement FromBytes(BinaryReader br)
        {
            return new ContactElement
            {
                Flag1 = br.ReadBoolean(),
                Flag2 = br.ReadBoolean(),
                Vectors = new List<Vector3>
                {
                    Vector3.FromBytes(br),
                    Vector3.FromBytes(br),
                    Vector3.FromBytes(br)
                },
                Value = br.ReadSingle()
            };
        }
    }
}
