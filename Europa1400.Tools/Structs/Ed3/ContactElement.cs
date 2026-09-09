using System.Collections.Generic;
using System.IO;
using Europa1400.Tools.Structs;

namespace Europa1400.Tools.Structs.Ed3
{
    public class ContactElement
    {
        public bool Flag1 { get; set; }
        public bool Flag2 { get; set; }
        public List<Vector3Struct> Vectors { get; set; }
        public float Value { get; set; }

        public static ContactElement FromBytes(BinaryReader br)
        {
            return new ContactElement
            {
                Flag1 = br.ReadBoolean(),
                Flag2 = br.ReadBoolean(),
                Vectors = new List<Vector3Struct>
                {
                    Vector3Struct.FromBytes(br),
                    Vector3Struct.FromBytes(br),
                    Vector3Struct.FromBytes(br)
                },
                Value = br.ReadSingle()
            };
        }
    }
}
