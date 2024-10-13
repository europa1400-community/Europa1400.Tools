using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Bgf;

internal class BgfModelStruct
{
    internal required uint VertexCount { get; init; }
    internal required uint PolygonCount { get; init; }
    internal required IEnumerable<Vector3Struct> Vertices { get; init; }
    internal required IEnumerable<BgfPolygonStruct> Polygons { get; init; }

    internal static BgfModelStruct FromBytes(BinaryReader br)
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
