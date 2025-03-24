namespace Europa1400.Tools.Structs.Bgf;

public class BgfTextureCoordinatesStruct
{
    public required float U { get; init; }
    public required float V { get; init; }
    public required float W { get; init; }

    public static BgfTextureCoordinatesStruct FromBytes(BinaryReader br)
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