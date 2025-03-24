using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Bgf;

public class BgfModelStruct
{
    public required uint VertexCount { get; init; }
    public required uint PolygonCount { get; init; }
    public required Vector3Struct[] Vertices { get; init; }
    public required BgfPolygonStruct[] Polygons { get; init; }

    public static BgfModelStruct FromBytes(BinaryReader br)
    {
        br.SkipRequiredByte(0x19);
        var vertexCount = br.ReadUInt32();
        br.SkipRequiredByte(0x1A);
        var polygonCount = br.ReadUInt32();
        br.SkipRequiredByte(0x1B);
        var vertices = br.ReadArray(Vector3Struct.FromBytes, vertexCount);
        br.SkipRequiredBytes(0x1C, 0x1D);
        var polygons = br.ReadArray(BgfPolygonStruct.FromBytes, polygonCount);

        return new BgfModelStruct
        {
            VertexCount = vertexCount,
            PolygonCount = polygonCount,
            Vertices = vertices,
            Polygons = polygons
        };
    }
}