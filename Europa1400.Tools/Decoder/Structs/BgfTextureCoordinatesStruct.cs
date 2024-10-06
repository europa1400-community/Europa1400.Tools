namespace Europa1400.Tools.Decoder.Structs;

public class BgfTextureCoordinatesStruct
{
    public float U { get; init; }
    public float V { get; init; }
    public float W { get; init; }

    public static BgfTextureCoordinatesStruct FromBytes(byte[] data)
    {
        var ms = new MemoryStream(data);
        var br = new BinaryReader(ms);

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
