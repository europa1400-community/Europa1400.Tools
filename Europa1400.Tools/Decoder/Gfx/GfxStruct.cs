using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Gfx;

internal class GfxStruct
{
    internal required uint ShapebankCount { get; init; }

    internal required IEnumerable<ShapebankDefinitionStruct> ShapebankDefinitions { get; init; }

    internal static GfxStruct FromBytes(BinaryReader br)
    {
        var shapebankCount = br.ReadUInt32();
        var shapebankDefinitions = br.ReadArray(ShapebankDefinitionStruct.FromBytes, shapebankCount);

        return new GfxStruct
        {
            ShapebankCount = shapebankCount,
            ShapebankDefinitions = shapebankDefinitions
        };
    }
}
