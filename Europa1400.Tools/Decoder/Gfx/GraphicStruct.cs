using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Gfx;

internal class GraphicStruct
{
    public required uint Size { get; init; }

    public required ushort Magic1 { get; init; }

    public required ushort Width { get; init; }

    public required ushort Magic2 { get; init; }

    public required ushort Height { get; init; }

    public required ushort Magic3 { get; init; }

    public required ushort Magic4 { get; init; }

    public required ushort Magic5 { get; init; }

    public required ushort Width2 { get; init; }

    public required ushort Height2 { get; init; }

    public required ushort Magic6 { get; init; }

    public required ushort Magic7 { get; init; }

    public required ushort Magic8 { get; init; }

    public required ushort Magic9 { get; init; }

    public required ushort Magic10 { get; init; }

    public required ushort Magic11 { get; init; }

    public required ushort Magic12 { get; init; }

    public required ushort Magic13 { get; init; }

    public required uint Magic14 { get; init; }

    public required uint SizeWithoutFooter { get; init; }

    public required uint Magic15 { get; init; }

    public required byte[]? PixelData { get; init; }

    public required List<GraphicRowStruct>? GraphicRows { get; init; }

    public required uint[]? FooterData { get; init; }

    public static GraphicStruct FromBytes(BinaryReader br)
    {
        var size = br.ReadUInt32();
        var magic1 = br.ReadUInt16();
        var width = br.ReadUInt16();
        var magic2 = br.ReadUInt16();
        var height = br.ReadUInt16();
        var magic3 = br.ReadUInt16();
        var magic4 = br.ReadUInt16();
        var magic5 = br.ReadUInt16();
        var width2 = br.ReadUInt16();
        var height2 = br.ReadUInt16();
        var magic6 = br.ReadUInt16();
        var magic7 = br.ReadUInt16();
        var magic8 = br.ReadUInt16();
        var magic9 = br.ReadUInt16();
        var magic10 = br.ReadUInt16();
        var magic11 = br.ReadUInt16();
        var magic12 = br.ReadUInt16();
        var magic13 = br.ReadUInt16();
        var magic14 = br.ReadUInt32();
        var sizeWithoutFooter = br.ReadUInt32();
        var magic15 = br.ReadUInt32();

        byte[]? pixelData = null;
        List<GraphicRowStruct>? graphicsRows = null;
        uint[]? footerData = null;

        if (sizeWithoutFooter > 0)
        {
            pixelData = br.ReadBytes(width * height * 3);
            graphicsRows = Enumerable.Range(0, height).Select(_ => GraphicRowStruct.FromBytes(br)).ToList();
            footerData = br.ReadUInt32s((int)(size - sizeWithoutFooter) / 4).Select(b => b).ToArray();
        }

        return new GraphicStruct
        {
            Size = size,
            Magic1 = magic1,
            Width = width,
            Height = height,
            Magic2 = magic2,
            Magic3 = magic3,
            Magic4 = magic4,
            Magic5 = magic5,
            Width2 = width2,
            Height2 = height2,
            Magic6 = magic6,
            Magic7 = magic7,
            Magic8 = magic8,
            Magic9 = magic9,
            Magic10 = magic10,
            Magic11 = magic11,
            Magic12 = magic12,
            Magic13 = magic13,
            Magic14 = magic14,
            SizeWithoutFooter = sizeWithoutFooter,
            Magic15 = magic15,
            PixelData = pixelData,
            GraphicRows = graphicsRows,
            FooterData = footerData
        };
    }
}
