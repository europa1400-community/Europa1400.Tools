using System.Linq;
using Europa1400.Tools.Structs.Bgf;

namespace Europa1400.Tools.Gltf;

internal class GltfPreConvertData
{
    internal required uint[][] Faces { get; init; }
    internal required byte[] TextureIndices { get; init; }
    internal required float[][] Vertices { get; init; }
    internal required float[][] Normals { get; init; }
    internal required float[][][] TexCoords { get; init; }

    internal static GltfPreConvertData FromBgf(BgfStruct bgf)
    {
        var visualFaces = BgfVisualGeometry.GetFaces(bgf);
        var faces = visualFaces
            .Select(e => new[] { e.A, e.B, e.C })
            .ToArray();
        var textureIndices = visualFaces
            .Select(e => (byte)e.TextureIndex)
            .ToArray();
        var vertices = bgf.MappingObject.VertexMappings
            .Select(e =>
            {
                var p = BgfVertexMapping.ToExport(e.Vertex1);
                return new[] { p.X, p.Y, p.Z };
            })
            .ToArray();
        var normals = bgf.MappingObject.VertexMappings
            .Select(e =>
            {
                var n = BgfVertexMapping.ToExport(e.Vertex2);
                return new[] { n.X, n.Y, n.Z };
            })
            .ToArray();
        var texCoords = visualFaces
            .Select(e => new[]
            {
                new[] { e.Uv1.X, e.Uv1.Y },
                new[] { e.Uv2.X, e.Uv2.Y },
                new[] { e.Uv3.X, e.Uv3.Y }
            })
            .ToArray();

        return new GltfPreConvertData
        {
            Faces = faces,
            TextureIndices = textureIndices,
            Vertices = vertices,
            Normals = normals,
            TexCoords = texCoords
        };
    }
}