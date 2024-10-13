using Europa1400.Tools.Extensions;
using System.Text;

namespace Europa1400.Tools.Decoder.Gfx;

internal class ShapebankStruct
{
    internal static IEnumerable<byte> ShapbankConst => Encoding.Latin1.GetBytes("SHAPBANK");

    internal required byte Unknown1 { get; init; }

    internal required byte Unknown2 { get; init; }

    internal required ushort GraphicsCount { get; init; }

    internal required IEnumerable<ushort> UnknownData1 { get; init; }

    internal required uint Size { get; init; }

    internal required uint Unknown3 { get; init; }

    internal required ushort SizeWithoutFooter { get; init; }

    internal required ushort Unknown4 { get; init; }

    internal required IEnumerable<uint> Offsets { get; init; }

    internal required IEnumerable<GraphicStruct> Graphics { get; init; }

    internal required IEnumerable<byte>? Footer { get; init; }

    public static ShapebankStruct FromBytes(BinaryReader br)
    {
        var magic1 = br.ReadByte();
        var magic2 = br.ReadByte();
        var graphicsCount = br.ReadUInt16();
        var magicData1 = br.ReadUInt16s(2).ToArray();
        var size = br.ReadUInt32();
        var magic3 = br.ReadUInt32();
        var sizeWithoutFooter = br.ReadUInt16();
        var magic4 = br.ReadUInt16();
        var offsets = br.ReadPaddedUInt32Array(graphicsCount, 0x800).ToArray();
        var graphics = br.ReadArray(GraphicStruct.FromBytes, graphicsCount);
        var footer = sizeWithoutFooter > 0 ? br.ReadBytes(graphicsCount * 8) : null;

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
