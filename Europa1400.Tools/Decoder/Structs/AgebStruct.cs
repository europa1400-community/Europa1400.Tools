namespace Europa1400.Tools.Decoder.Structs;

public class AgebStruct
{
    public required IEnumerable<AgebBuildingStruct> Buildings { get; init; }

    public static AgebStruct FromBytes(byte[] data)
    {
        var buildings = new List<AgebBuildingStruct>();
        var offset = 0;

        while (offset < data.Length)
        {
            var building = AgebBuildingStruct.FromBytes(data[offset..]);
            buildings.Add(building);
            offset += 64;
        }

        return new AgebStruct
        {
            Buildings = buildings
        };
    }
}
