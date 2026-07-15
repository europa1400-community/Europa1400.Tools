using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfModelStruct
    {
        public uint VertexCount { get; set; }
        public uint PolygonCount { get; set; }
        public Vector3Struct[] Vertices { get; set; }
        public BgfPolygonStruct[] Polygons { get; set; }

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
}