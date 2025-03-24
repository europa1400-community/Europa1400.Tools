using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Ageb;

public class AgebStruct
{
    public required AgebBuildingStruct[] Buildings { get; init; }

    public static AgebStruct FromBytes(BinaryReader br)
    {
        var buildings = br.ReadArray(AgebBuildingStruct.FromBytes, 88);

        return new AgebStruct
        {
            Buildings = buildings
        };
    }
}