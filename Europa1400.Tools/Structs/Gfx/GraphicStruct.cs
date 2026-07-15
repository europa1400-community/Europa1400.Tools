using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Gfx
{
    public class GraphicStruct
    {
        public uint Size { get; set; }
        public ushort Unknown1 { get; set; }
        public ushort Width { get; set; }
        public ushort Unknown2 { get; set; }
        public ushort Height { get; set; }
        public ushort Unknown3 { get; set; }
        public ushort Unknown4 { get; set; }
        public ushort Unknown5 { get; set; }
        public ushort Width2 { get; set; }
        public ushort Height2 { get; set; }
        public ushort Unknown6 { get; set; }
        public ushort Unknown7 { get; set; }
        public ushort Unknown8 { get; set; }
        public ushort Unknown9 { get; set; }
        public ushort Unknown10 { get; set; }
        public ushort Unknown11 { get; set; }
        public ushort Unknown12 { get; set; }
        public ushort Unknown13 { get; set; }
        public uint Unknown14 { get; set; }
        public uint SizeWithoutFooter { get; set; }
        public uint Unknown15 { get; set; }

        public byte[]? PixelData { get; set; }
        public GraphicRowStruct[]? GraphicRows { get; set; }
        public uint[]? FooterData { get; set; }

        public static GraphicStruct FromBytes(BinaryReader br, long address)
        {
            var pos = br.BaseStream.Position;
            br.BaseStream.Position = address;

            var size = br.ReadUInt32();
            var unknown1 = br.ReadUInt16();
            var width = br.ReadUInt16();
            var unknown2 = br.ReadUInt16();
            var height = br.ReadUInt16();
            var unknown3 = br.ReadUInt16();
            var unknown4 = br.ReadUInt16();
            var unknown5 = br.ReadUInt16();
            var width2 = br.ReadUInt16();
            var height2 = br.ReadUInt16();
            var unknown6 = br.ReadUInt16();
            var unknown7 = br.ReadUInt16();
            var unknown8 = br.ReadUInt16();
            var unknown9 = br.ReadUInt16();
            var unknown10 = br.ReadUInt16();
            var unknown11 = br.ReadUInt16();
            var unknown12 = br.ReadUInt16();
            var unknown13 = br.ReadUInt16();
            var unknown14 = br.ReadUInt32();
            var sizeWithoutFooter = br.ReadUInt32();
            var unknown15 = br.ReadUInt32();

            var pixelData = sizeWithoutFooter == 0 ? br.ReadBytes(width * height * 3) : null;
            var graphicsRows = sizeWithoutFooter > 0 ? br.ReadArray(GraphicRowStruct.FromBytes, height) : null;
            var footerData = sizeWithoutFooter > 0 ? br.ReadUInt32s((size - sizeWithoutFooter) / 4) : null;

            br.BaseStream.Position = pos;

            return new GraphicStruct
            {
                Size = size,
                Unknown1 = unknown1,
                Width = width,
                Height = height,
                Unknown2 = unknown2,
                Unknown3 = unknown3,
                Unknown4 = unknown4,
                Unknown5 = unknown5,
                Width2 = width2,
                Height2 = height2,
                Unknown6 = unknown6,
                Unknown7 = unknown7,
                Unknown8 = unknown8,
                Unknown9 = unknown9,
                Unknown10 = unknown10,
                Unknown11 = unknown11,
                Unknown12 = unknown12,
                Unknown13 = unknown13,
                Unknown14 = unknown14,
                SizeWithoutFooter = sizeWithoutFooter,
                Unknown15 = unknown15,
                PixelData = pixelData,
                GraphicRows = graphicsRows,
                FooterData = footerData
            };
        }
    }
}