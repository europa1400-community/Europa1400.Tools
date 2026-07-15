using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Gfx
{
    public class TransparencyBlockStruct
    {
        public uint Size { get; set; }
        public uint PixelCount { get; set; }
        public byte[] Data { get; set; }

        public static TransparencyBlockStruct FromBytes(BinaryReader br)
        {
            var size = br.ReadUInt32();
            var pixelCount = br.ReadUInt32();
            var data = br.ReadBytes(pixelCount * 3);

            return new TransparencyBlockStruct
            {
                Size = size,
                PixelCount = pixelCount,
                Data = data
            };
        }
    }
}