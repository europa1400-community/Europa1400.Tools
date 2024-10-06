namespace Europa1400.Tools.Decoder.Gfx;

internal class GfxStruct
{
    public required uint ShapebankCount { get; init; }

    public required List<ShapebankDefinitionStruct> ShapebankDefinitions { get; init; }

    public static GfxStruct FromBytes(BinaryReader br)
    {
        var shapebankCount = br.ReadUInt32();
        var shapebankDefinitions = Enumerable.Range(0, (int)shapebankCount).Select(_ => ShapebankDefinitionStruct.FromBytes(br)).ToList();

        return new GfxStruct
        {
            ShapebankCount = shapebankCount,
            ShapebankDefinitions = shapebankDefinitions
        };
    }
}
