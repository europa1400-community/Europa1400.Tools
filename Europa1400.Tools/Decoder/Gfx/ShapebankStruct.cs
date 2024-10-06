using Europa1400.Tools.Extensions;
using System.Text;

namespace Europa1400.Tools.Decoder.Gfx;

internal class ShapebankStruct
{
    public static byte[] ShapbankConst => Encoding.ASCII.GetBytes("SHAPBANK");

    public required byte Magic1 { get; init; }

    public required byte Magic2 { get; init; }

    public required byte[] Zeroes { get; init; }

    public required ushort GraphicsCount { get; init; }

    public required ushort[] MagicData1 { get; init; }

    public required uint Size { get; init; }

    public required uint Magic3 { get; init; }

    public required byte[] Zeroes2 { get; init; }

    public required ushort SizeWithoutFooter { get; init; }

    public required byte[] Zeroes3 { get; init; }

    public required ushort Magic4 { get; init; }

    public required uint[] Offsets { get; init; }

    public required List<GraphicStruct> Graphics { get; init; }

    public required byte[]? Footer { get; init; }

    public static ShapebankStruct FromBytes(BinaryReader br)
    {
        var magic1 = br.ReadByte();
        var magic2 = br.ReadByte();
        var zeroes = br.ReadBytes(32);
        var graphicsCount = br.ReadUInt16();
        var magicData1 = br.ReadUInt16s(2).ToArray();
        var size = br.ReadUInt32();
        var magic3 = br.ReadUInt32();
        var zeroes2 = br.ReadBytes(6);
        var sizeWithoutFooter = br.ReadUInt16();
        var zeroes3 = br.ReadBytes(3);
        var magic4 = br.ReadUInt16();
        var offsets = br.ReadPaddedUInt32Array(graphicsCount, 0x800).ToArray();
        var graphics = Enumerable.Range(0, graphicsCount).Select(_ => GraphicStruct.FromBytes(br)).ToList();
        var footer = sizeWithoutFooter > 0 ? br.ReadBytes(graphicsCount * 8) : null;

        return new ShapebankStruct
        {
            Magic1 = magic1,
            Magic2 = magic2,
            Zeroes = zeroes,
            GraphicsCount = graphicsCount,
            MagicData1 = magicData1,
            Size = size,
            Magic3 = magic3,
            Zeroes2 = zeroes2,
            SizeWithoutFooter = sizeWithoutFooter,
            Zeroes3 = zeroes3,
            Magic4 = magic4,
            Offsets = offsets,
            Graphics = graphics,
            Footer = footer
        };
    }
}
