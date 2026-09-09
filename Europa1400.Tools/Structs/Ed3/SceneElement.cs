using System;
using System.Collections.Generic;
using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Ed3
{
    public class SceneElement
    {
        public int OnesCount { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int? Height { get; set; }
        public int Type { get; set; }
        public CityElement? CityElement { get; set; }
        public ContactElement? ContactElement { get; set; }
        public DummyElement? DummyElement { get; set; }
        public CameraElement? CameraElement { get; set; }
        public ObjectElement? ObjectElement { get; set; }
        public LightElement? LightElement { get; set; }
        public int SkipLength { get; set; }
        public List<ScriptElement> ScriptElements { get; set; }

        public static SceneElement FromBytes(BinaryReader br, byte grandparentType)
        {
            var element = new SceneElement
            {
                OnesCount = br.SkipWhile(1),
                Name = br.ReadCString(),
                Width = br.ReadInt32()
            };

            if (grandparentType != 0xAF)
                element.Height = br.ReadInt32();

            element.Type = br.ReadInt32();

            var isCity = element.Width == 0x40 || element.Width == 0x80 || element.Width == 0x100;
            if (isCity)
                element.CityElement = CityElement.FromBytes(br, grandparentType, element.Width, element.Height ?? 0);
            else if (element.Type == 0x00)
                element.ContactElement = ContactElement.FromBytes(br);
            else if (element.Type == 0x02)
                element.DummyElement = DummyElement.FromBytes(br);
            else if (element.Type == 0x03)
                element.CameraElement = CameraElement.FromBytes(br);
            else if (element.Type == 0x04)
                element.ObjectElement = ObjectElement.FromBytes(br, grandparentType);
            else if (element.Type >= 0x05 && element.Type <= 0x08)
                element.LightElement = LightElement.FromBytes(br, grandparentType);

            element.SkipLength = br.SkipWhile(0);

            element.ScriptElements = new List<ScriptElement>();
            while (br.BaseStream.Position < br.BaseStream.Length - 4)
            {
                var startPos = br.BaseStream.Position;
                var peekBytes = br.ReadBytes(4);
                br.BaseStream.Position = startPos;
                var possibleCount = BitConverter.ToInt32(peekBytes, 0);
                if (possibleCount <= 0 || possibleCount > 50)
                    break;
                try
                {
                    var scriptElement = ScriptElement.FromBytes(br);
                    element.ScriptElements.Add(scriptElement);
                }
                catch
                {
                    br.BaseStream.Position = startPos;
                    break;
                }
            }

            return element;
        }
    }
}
