namespace Europa1400.Tools.Decoder.Bgf;

internal class BgfFaceStruct
{
    internal required uint A { get; init; }
    internal required uint B { get; init; }
    internal required uint C { get; init; }

    internal static BgfFaceStruct FromBytes(BinaryReader br)
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
