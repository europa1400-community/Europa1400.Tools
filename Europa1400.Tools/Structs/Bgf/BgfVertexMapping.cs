using System.IO;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfVertexMapping
    {
        public Vector3Struct Vertex1 { get; private set; }
        public Vector3Struct Vertex2 { get; private set; }

        public Vector3Struct Vertex1Transformed => new Vector3Struct
        {
            X = Vertex1.X, Y = Vertex1.Y, Z = -Vertex1.Z
        };

        public Vector3Struct Vertex2Transformed => new Vector3Struct
        {
            X = Vertex2.X, Y = Vertex2.Y, Z = -Vertex2.Z
        };

        public static BgfVertexMapping FromBytes(BinaryReader br)
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
}