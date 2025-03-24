namespace Europa1400.Tools.Structs.Ageb;

public class AgebBuildingCoordinatesStruct
{
    public required byte X { get; init; }

    public required byte Y { get; init; }

    public required byte Z { get; init; }

    public static AgebBuildingCoordinatesStruct FromBytes(BinaryReader br)
    {
        var x = br.ReadByte();
        var y = br.ReadByte();
        var z = br.ReadByte();

        return new AgebBuildingCoordinatesStruct
        {
            X = x,
            Y = y,
            Z = z
        };
    }
}