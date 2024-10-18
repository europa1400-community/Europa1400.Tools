namespace Europa1400.Tools.Decoder.Bgf;

internal class BgfVertexMapping
{
    internal required Vector3Struct Vertex1 { get; init; }
    internal required Vector3Struct Vertex2 { get; init; }
    internal Vector3Struct Vertex1Transformed => new()
    {
        X = Vertex1.X, Y = Vertex1.Y, Z = -Vertex1.Z
    };
    internal Vector3Struct Vertex2Transformed => new()
    {
        X = Vertex2.X, Y = Vertex2.Y, Z = -Vertex2.Z
    };

    internal static BgfVertexMapping FromBytes(BinaryReader br)
    {
        var vertex1 = Vector3Struct.FromBytes(br);
        var vertex2 = Vector3Struct.FromBytes(br);

        return new BgfVertexMapping
        {
            Vertex1 = vertex1,
            Vertex2 = vertex2
        };
    }
}
