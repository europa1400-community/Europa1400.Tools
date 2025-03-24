using System.IO;
using System.Linq;
using System.Text;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Gfx
{
    public class ShapebankStruct
    {
        public static byte[] ShapbankConst => Encoding.ASCII.GetBytes("SHAPBANK");
        public byte Unknown1 { get; private set; }
        public byte Unknown2 { get; private set; }
        public ushort GraphicsCount { get; private set; }
        public ushort[] UnknownData1 { get; private set; }
        public uint Size { get; private set; }
        public uint Unknown3 { get; private set; }
        public ushort SizeWithoutFooter { get; private set; }
        public ushort Unknown4 { get; private set; }
        public uint[] Offsets { get; private set; }
        public GraphicStruct[] Graphics { get; private set; }
        public byte[]? Footer { get; private set; }

        public static ShapebankStruct FromBytes(BinaryReader br, long address)
        {
            var pos = br.BaseStream.Position;
            br.BaseStream.Position = address;

            br.Skip(8);
            var magic1 = br.ReadByte();
            var magic2 = br.ReadByte();
            br.Skip(32);
            var graphicsCount = br.ReadUInt16();
            var magicData1 = br.ReadUInt16s(2).ToArray();
            var size = br.ReadUInt32();
            var magic3 = br.ReadUInt32();
            br.Skip(6);
            var sizeWithoutFooter = br.ReadUInt16();
            br.Skip(3);
            var magic4 = br.ReadUInt16();
            var offsets = br.ReadPaddedUInt32Array(graphicsCount, 0x800).ToArray();
            var graphics = br.ReadArray((reader, idx) => GraphicStruct.FromBytes(reader, address + offsets[idx]),
                graphicsCount);
            var footer = sizeWithoutFooter > 0 ? br.ReadBytes(size - sizeWithoutFooter) : null;

            br.BaseStream.Position = pos;

            return new ShapebankStruct
            {
                Unknown1 = magic1,
                Unknown2 = magic2,
                GraphicsCount = graphicsCount,
                UnknownData1 = magicData1,
                Size = size,
                Unknown3 = magic3,
                SizeWithoutFooter = sizeWithoutFooter,
                Unknown4 = magic4,
                Offsets = offsets,
                Graphics = graphics,
                Footer = footer
            };
        }
    }
}