namespace Europa1400.Tools.Decoder.Structs;

public class AgebBuildingCoordinatesStruct
{
    public byte X { get; init; }

    public byte Y { get; init; }

    public byte Z { get; init; }

    public static AgebBuildingCoordinatesStruct FromBytes(byte[] data)
    {
        var ms = new MemoryStream(data);
        var br = new BinaryReader(ms);

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
