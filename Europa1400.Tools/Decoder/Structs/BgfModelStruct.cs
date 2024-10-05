namespace Europa1400.Tools.Decoder.Structs;

public class BgfModelStruct
{
    public int VertexCount { get; init; }
    public int PolygonCount { get; init; }
    public required IEnumerable<Vector3Struct> Vertices { get; init; }
    public required IEnumerable<BgfPolygonStruct> Polygons { get; init; }

    public static BgfModelStruct FromBytes(byte[] data)
    {
        var ms = new MemoryStream(data);
        var br = new BinaryReader(ms);

        return FromBytes(br);
    }

    public static BgfModelStruct FromBytes(BinaryReader br)
    {
        br.SkipRequiredByte(0x19);
        var vertexCount = br.ReadInt32();
        br.SkipRequiredByte(0x1A);
        var polygonCount = br.ReadInt32();
        br.SkipRequiredByte(0x1B);
        var vertices = Enumerable.Range(0, vertexCount).Select(_ => Vector3Struct.FromBytes(br));
        br.SkipRequiredBytes(0x1C, 0x1D);
        var polygons = Enumerable.Range(0, polygonCount).Select(_ => BgfPolygonStruct.FromBytes(br));

        return new BgfModelStruct
        {
            VertexCount = vertexCount,
            PolygonCount = polygonCount,
            Vertices = vertices,
            Polygons = polygons
        };
    }
}
