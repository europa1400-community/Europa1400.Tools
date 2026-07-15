using System.Collections.Generic;
using System.IO;

namespace Europa1400.Tools.Decoder.Ed3
{
    public class CameraElement
    {
        public bool Flag1 { get; set; }
        public Transform Transform { get; set; }
        public bool Flag2 { get; set; }
        public List<Transform> Transforms { get; set; }

        public static CameraElement FromBytes(BinaryReader br)
        {
            var element = new CameraElement
            {
                Flag1 = br.ReadBoolean(),
                Transform = Transform.FromBytes(br),
                Flag2 = br.ReadBoolean()
            };

            element.Transforms = new List<Transform>(11);
            for (int i = 0; i < 11; i++)
                element.Transforms.Add(Transform.FromBytes(br));

            return element;
        }
    }
}
