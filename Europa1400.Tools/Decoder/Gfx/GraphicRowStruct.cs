namespace Europa1400.Tools.Decoder.Gfx;

internal class GraphicRowStruct
{
    public required uint BlockCount { get; init; }

    public required List<TransparencyBlockStruct> TransparencyBlocks { get; init; }

    public static GraphicRowStruct FromBytes(BinaryReader br)
    {
        var blockCount = br.ReadUInt32();
        var transparency = Enumerable.Range(0, (int)blockCount).Select(_ => TransparencyBlockStruct.FromBytes(br)).ToList();

        return new GraphicRowStruct
        {
            BlockCount = blockCount,
            TransparencyBlocks = transparency
        };
    }
}
