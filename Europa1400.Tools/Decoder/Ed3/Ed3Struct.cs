using System.Collections.Generic;
using System.IO;

namespace Europa1400.Tools.Decoder.Ed3
{
    public class Ed3Struct
    {
        public byte Type { get; set; }
        public MainCameraElement MainCameraElement { get; set; }
        public List<SceneElement> SceneElements { get; set; }

        internal static Ed3Struct FromBytes(BinaryReader br)
        {
            var ed3 = new Ed3Struct();

            ed3.Type = br.ReadByte();

            var constBytes = br.ReadBytes(3);
            if (constBytes[0] != 0x00 || constBytes[1] != 0x6C || constBytes[2] != 0x3A)
                throw new InvalidDataException($"Invalid ED3 constant: expected 0x00 0x6C 0x3A, got {constBytes[0]:X2} {constBytes[1]:X2} {constBytes[2]:X2}");

            ed3.MainCameraElement = MainCameraElement.FromBytes(br, ed3.Type);

            ed3.SceneElements = new List<SceneElement>();
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                var peek = br.ReadByte();
                br.BaseStream.Position -= 1;

                if (peek == 0)
                    break;

                var element = SceneElement.FromBytes(br, ed3.Type);
                ed3.SceneElements.Add(element);
            }

            return ed3;
        }
    }
}
