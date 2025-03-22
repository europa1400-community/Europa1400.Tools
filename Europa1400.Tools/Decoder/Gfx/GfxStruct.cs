using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Gfx;

internal class GfxStruct
{
    internal required uint ShapebankCount { get; init; }
    internal required ShapebankDefinitionStruct[] ShapebankDefinitions { get; init; }

    internal static GfxStruct FromBytes(BinaryReader br)
    {
        var shapebankCount = br.ReadUInt32();

        var shapebankDefinitions = new List<ShapebankDefinitionStruct>();
        
        for (var i = 0; i < shapebankCount; i++)
        {
            var def = ShapebankDefinitionStruct.FromBytes(br);

            i += def.ChildShapebankCount;
            
            shapebankDefinitions.Add(def);
        }

        return new GfxStruct
        {
            ShapebankCount = shapebankCount,
            ShapebankDefinitions = shapebankDefinitions.ToArray()
        };
    }
}
