using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Gfx;

public class GraphicRowStruct
{
    public required uint BlockCount { get; init; }
    public required TransparencyBlockStruct[] TransparencyBlocks { get; init; }

    public static GraphicRowStruct FromBytes(BinaryReader br)
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