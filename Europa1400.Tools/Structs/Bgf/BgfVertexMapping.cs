using System.IO;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfVertexMapping
    {
        public Vector3Struct Vertex1 { get; set; }
        public Vector3Struct Vertex2 { get; set; }

        public Vector3Struct Vertex1Transformed => ToExport(Vertex1);
        public Vector3Struct Vertex2Transformed => ToExport(Vertex2);

        /// <summary>
        /// MappingObject vertices are already Y-up (+Y height, +X right).
        /// Negate Z for the DirectX (left-handed) to glTF/OBJ (right-handed) convention.
        /// </summary>
        public static Vector3Struct ToExport(Vector3Struct v)
        {
            return new Vector3Struct
            {
                X = v.X,
                Y = v.Y,
                Z = -v.Z
            };
        }

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