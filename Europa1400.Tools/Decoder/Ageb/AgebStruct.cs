using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Ageb;

internal class AgebStruct
{
    internal required IEnumerable<AgebBuildingStruct> Buildings { get; init; }

    internal static AgebStruct FromBytes(BinaryReader br)
    {
        var buildings = br.ReadArray(AgebBuildingStruct.FromBytes, 88);

        return new AgebStruct
        {
            Buildings = buildings
        };
    }
}
