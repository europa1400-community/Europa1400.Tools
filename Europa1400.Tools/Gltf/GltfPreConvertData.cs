using Europa1400.Tools.Decoder.Bgf;

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
        var faces = bgf.MappingObject.PolygonMappings
            .Select(e => new[] { e.Face.A, e.Face.B, e.Face.C })
            .ToArray();
        var textureIndices = bgf.MappingObject.PolygonMappings
            .Select(e => e.TextureIndex)
            .ToArray();
        var vertices = bgf.MappingObject.VertexMappings
            .Select(e => new[] { e.Vertex1.X, e.Vertex1.Y, -e.Vertex1.Z })
            .ToArray();
        var normals = bgf.MappingObject.VertexMappings
            .Select(e => new[] { e.Vertex2.X, e.Vertex2.Y, -e.Vertex2.Z })
            .ToArray();
        var texCoords = bgf.MappingObject.PolygonMappings
            .Select(e => new[]
            {
                new[] { e.TextureMapping.TextureCoordinates1.U, e.TextureMapping.TextureCoordinates1.V },
                new[] { e.TextureMapping.TextureCoordinates2.U, e.TextureMapping.TextureCoordinates2.V },
                new[] { e.TextureMapping.TextureCoordinates3.U, e.TextureMapping.TextureCoordinates3.V }
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