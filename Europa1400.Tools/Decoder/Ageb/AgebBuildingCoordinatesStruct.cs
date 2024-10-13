namespace Europa1400.Tools.Decoder.Ageb;

internal class AgebBuildingCoordinatesStruct
{
    internal required byte X { get; init; }

    internal required byte Y { get; init; }

    internal required byte Z { get; init; }

    internal static AgebBuildingCoordinatesStruct FromBytes(BinaryReader br)
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
