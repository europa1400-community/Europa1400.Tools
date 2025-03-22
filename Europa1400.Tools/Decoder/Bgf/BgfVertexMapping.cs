namespace Europa1400.Tools.Decoder.Bgf;

internal class BgfVertexMapping
{
    internal required Vector3Struct Vertex1 { get; init; }
    internal required Vector3Struct Vertex2 { get; init; }

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