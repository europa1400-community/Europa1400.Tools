namespace Europa1400.Tools.Structs.Bgf;

public class BgfFaceStruct
{
    public required uint A { get; init; }
    public required uint B { get; init; }
    public required uint C { get; init; }

    public static BgfFaceStruct FromBytes(BinaryReader br)
    {
        var a = br.ReadUInt32();
        var b = br.ReadUInt32();
        var c = br.ReadUInt32();

        return new BgfFaceStruct
        {
            A = a,
            B = b,
            C = c
        };
    }
}