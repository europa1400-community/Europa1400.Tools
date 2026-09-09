using System.Collections.Generic;
using System.IO;

namespace Europa1400.Tools.Structs.Ed3
{
    public class DummyElement
    {
        public bool Flag1 { get; set; }
        public Transform Transform { get; set; }
        public bool Flag2 { get; set; }
        public List<Transform> Transforms { get; set; }

        public static DummyElement FromBytes(BinaryReader br)
        {
            var element = new DummyElement
            {
                Flag1 = br.ReadBoolean(),
                Transform = Transform.FromBytes(br),
                Flag2 = br.ReadBoolean()
            };

            var transformCount = element.Flag2 ? 11 : 10;
            element.Transforms = new List<Transform>(transformCount);
            for (int i = 0; i < transformCount; i++)
                element.Transforms.Add(Transform.FromBytes(br));

            return element;
        }
    }
}
