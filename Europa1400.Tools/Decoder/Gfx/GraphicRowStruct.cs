using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Gfx;

internal class GraphicRowStruct
{
    internal required uint BlockCount { get; init; }

    internal required IEnumerable<TransparencyBlockStruct> TransparencyBlocks { get; init; }

    internal static GraphicRowStruct FromBytes(BinaryReader br)
    {
        var blockCount = br.ReadUInt32();
        var transparency = br.ReadArray(TransparencyBlockStruct.FromBytes, blockCount);

        return new GraphicRowStruct
        {
            BlockCount = blockCount,
            TransparencyBlocks = transparency
        };
    }
}
