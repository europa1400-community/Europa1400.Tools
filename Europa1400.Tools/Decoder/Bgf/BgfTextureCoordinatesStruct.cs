namespace Europa1400.Tools.Decoder.Bgf;

internal class BgfTextureCoordinatesStruct
{
    internal required float U { get; init; }
    internal required float V { get; init; }
    internal required float W { get; init; }

    internal static BgfTextureCoordinatesStruct FromBytes(BinaryReader br)
    {
        var u = br.ReadSingle();
        var v = br.ReadSingle();
        var w = br.ReadSingle();

        return new BgfTextureCoordinatesStruct
        {
            U = u,
            V = v,
            W = w
        };
    }
}
