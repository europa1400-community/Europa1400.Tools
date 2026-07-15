using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Europa1400.Tools.Decoder.Ed3
{
    internal static class ReadHelper
    {
        public static string ReadCString(BinaryReader br)
        {
            var bytes = new List<byte>();
            byte b;
            while ((b = br.ReadByte()) != 0)
                bytes.Add(b);
            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        public static void SkipZeroes(BinaryReader br)
        {
            while (br.BaseStream.Position < br.BaseStream.Length && br.ReadByte() == 0)
            {
            }
            br.BaseStream.Position -= 1;
        }

        public static void SkipOnes(BinaryReader br)
        {
            while (br.BaseStream.Position < br.BaseStream.Length && br.ReadByte() == 1)
            {
            }
            br.BaseStream.Position -= 1;
        }

        public static List<float> ReadFloats(BinaryReader br, int count)
        {
            var floats = new List<float>(count);
            for (int i = 0; i < count; i++)
                floats.Add(br.ReadSingle());
            return floats;
        }

        public static List<byte> ReadBytesAsList(BinaryReader br, int count)
        {
            var bytes = new List<byte>(count);
            for (int i = 0; i < count; i++)
                bytes.Add(br.ReadByte());
            return bytes;
        }
    }
}
