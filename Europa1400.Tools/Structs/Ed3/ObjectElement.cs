using System.Collections.Generic;
using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Ed3
{
    public class ObjectElement
    {
        public bool Flag1 { get; set; }
        public bool Flag2 { get; set; }
        public bool Flag3 { get; set; }
        public byte ObjectElementNum1 { get; set; }
        public bool Flag4 { get; set; }
        public bool? Flag5 { get; set; }
        public bool? Flag6 { get; set; }
        public byte ObjectElementNum2 { get; set; }
        public bool Flag7 { get; set; }
        public bool HasObjectName { get; set; }
        public string? Name { get; set; }
        public Transform Transform { get; set; }
        public bool Flag8 { get; set; }
        public List<Transform> Transforms { get; set; }

        public static ObjectElement FromBytes(BinaryReader br, byte parentType)
        {
            var element = new ObjectElement
            {
                Flag1 = br.ReadBoolean()
            };

            br.ReadByte(); // zero1

            element.Flag2 = br.ReadBoolean();
            element.Flag3 = br.ReadBoolean();
            element.ObjectElementNum1 = br.ReadByte();
            element.Flag4 = br.ReadBoolean();

            if (parentType != 0xAF)
            {
                element.Flag5 = br.ReadBoolean();
                element.Flag6 = br.ReadBoolean();
            }

            element.ObjectElementNum2 = br.ReadByte();
            br.ReadByte(); // zero2
            element.Flag7 = br.ReadBoolean();
            br.ReadByte(); // zero3
            element.HasObjectName = br.ReadBoolean();
            br.ReadBytes(3); // zero4

            if (element.HasObjectName)
                element.Name = br.ReadCString();

            element.Transform = Transform.FromBytes(br);
            element.Flag8 = br.ReadBoolean();

            var transformCount = element.Flag8 ? 11 : 10;
            element.Transforms = new List<Transform>(transformCount);
            for (int i = 0; i < transformCount; i++)
                element.Transforms.Add(Transform.FromBytes(br));

            return element;
        }
    }
}
