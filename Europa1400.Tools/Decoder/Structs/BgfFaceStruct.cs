namespace Europa1400.Tools.Decoder.Structs;

public class BgfFaceStruct
{
    public int A { get; init; }
    public int B { get; init; }
    public int C { get; init; }

    public static BgfFaceStruct FromBytes(byte[] data)
    {
        var ms = new MemoryStream(data);
        var br = new BinaryReader(ms);

        return FromBytes(br);
    }

    public static BgfFaceStruct FromBytes(BinaryReader br)
    {
        var a = br.ReadInt32();
        var b = br.ReadInt32();
        var c = br.ReadInt32();

        return new BgfFaceStruct
        {
            A = a,
            B = b,
            C = c
        };
    }
}
