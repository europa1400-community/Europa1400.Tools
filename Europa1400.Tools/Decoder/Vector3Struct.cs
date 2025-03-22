namespace Europa1400.Tools.Decoder;

internal class Vector3Struct
{
    internal required float X { get; init; }
    internal required float Y { get; init; }
    internal required float Z { get; init; }

    internal static Vector3Struct FromBytes(BinaryReader br)
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
