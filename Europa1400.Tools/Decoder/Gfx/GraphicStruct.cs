﻿using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Gfx;

internal class GraphicStruct
{
    internal required uint Size { get; init; }
    internal required ushort Unknown1 { get; init; }
    internal required ushort Width { get; init; }
    internal required ushort Unknown2 { get; init; }
    internal required ushort Height { get; init; }
    internal required ushort Unknown3 { get; init; }
    internal required ushort Unknown4 { get; init; }
    internal required ushort Unknown5 { get; init; }
    internal required ushort Width2 { get; init; }
    internal required ushort Height2 { get; init; }
    internal required ushort Unknown6 { get; init; }
    internal required ushort Unknown7 { get; init; }
    internal required ushort Unknown8 { get; init; }
    internal required ushort Unknown9 { get; init; }
    internal required ushort Unknown10 { get; init; }
    internal required ushort Unknown11 { get; init; }
    internal required ushort Unknown12 { get; init; }
    internal required ushort Unknown13 { get; init; }
    internal required uint Unknown14 { get; init; }
    internal required uint SizeWithoutFooter { get; init; }
    internal required uint Unknown15 { get; init; }
    internal required byte[]? PixelData { get; init; }
    internal required GraphicRowStruct[]? GraphicRows { get; init; }
    internal required uint[]? FooterData { get; init; }

    internal static GraphicStruct FromBytes(BinaryReader br)
    {
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

        var pixelData = sizeWithoutFooter > 0 ? br.ReadBytes(width * height * 3) : null;
        var graphicsRows = sizeWithoutFooter > 0 ? br.ReadArray(GraphicRowStruct.FromBytes, height) : null;
        var footerData = sizeWithoutFooter > 0 ? br.ReadUInt32s((size - sizeWithoutFooter) / 4) : null;

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
