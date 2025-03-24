namespace Europa1400.Tools.Structs;

public class Vector3Struct
{
    public required float X { get; init; }
    public required float Y { get; init; }
    public required float Z { get; init; }

    public static Vector3Struct FromBytes(BinaryReader br)
    {
        var x = br.ReadSingle();
        var y = br.ReadSingle();
        var z = br.ReadSingle();

        return new Vector3Struct
        {
            X = x,
            Y = y,
            Z = z
        };
    }
}