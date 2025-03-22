using System.Numerics;
using Europa1400.Tools.Decoder.Baf;
using Europa1400.Tools.Decoder.Bgf;
using Europa1400.Tools.Decoder.Txs;
using Europa1400.Tools.Extensions;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using AlphaMode = SharpGLTF.Materials.AlphaMode;

namespace Europa1400.Tools.Gltf;

internal static class GltfUtil
{
    private static List<BgfTextureStruct> GetReorderedTextures(BgfStruct bgf)
    {
        var textures = new List<BgfTextureStruct>();
        var missingTextures = new List<BgfTextureStruct>();

        var textureNames = bgf.Textures
            .Select(e => e.Name.NormalizeName());
        var footerNames = bgf.Footer.TextureNames
            .Select(e => e.Name)
            .Where(e => textureNames.Contains(e.NormalizeName()));
        var normalizedFooterNames = footerNames
            .Select(e => e.NormalizeName())
            .ToList();

        foreach (var texture in bgf.Textures)
        {
            var normalizedName = texture.Name.NormalizeName();
            if (!normalizedFooterNames.Contains(normalizedName))
            {
                missingTextures.Add(texture);
                continue;
            }

            textures.Add(texture);
        }

        textures
            .Sort((e, f) => normalizedFooterNames.IndexOf(e.Name.NormalizeName())
                .CompareTo(normalizedFooterNames.IndexOf(f.Name.NormalizeName())));

        textures.AddRange(missingTextures);
        return textures;
    }

    internal static GltfModelData GetModelData(BgfStruct bgf, BafStruct[]? bafs, TxsStruct? txs,
        Dictionary<string, string> extractedTextures)
    {
        var reorderedTextures = GetReorderedTextures(bgf);
        var primitives = reorderedTextures
            .Select((texture, textureIndex) => GetPrimitiveData(bgf, texture, textureIndex, extractedTextures, txs))
            .Where(e => e is not null)
            .Cast<GltfPrimitiveData>()
            .ToList();

        return new GltfModelData
        {
            Name = bgf.Header.Name,
            Primitives = primitives.ToArray()
        };
    }

    private static GltfPrimitiveData? GetPrimitiveData(BgfStruct bgf, BgfTextureStruct texture,
        int textureIndex, Dictionary<string, string> extractedTextures, TxsStruct? txs)
    {
        var textureName = texture.Name;
        var isTransparentTexture = texture.HasTransparency;
        var texturePath = GetTexturePath(textureName, extractedTextures, txs);

        if (texturePath is null) return null;

        var faces = bgf.MappingObject.PolygonMappings
            .Where(e => e.TextureIndex == textureIndex).ToArray();

        if (faces.Length == 0) return null;

        return new GltfPrimitiveData
        {
            TextureName = textureName,
            TexturePath = texturePath,
            Triangles = (from face in faces
                let vertex1 =
                    new GltfVertexData
                    {
                        Position = new Vector3(bgf.MappingObject.VertexMappings[face.Face.A].Vertex1.X,
                            bgf.MappingObject.VertexMappings[face.Face.A].Vertex1.Y,
                            -bgf.MappingObject.VertexMappings[face.Face.A].Vertex1.Z),
                        Normal = new Vector3(bgf.MappingObject.VertexMappings[face.Face.A].Vertex2.X,
                            bgf.MappingObject.VertexMappings[face.Face.A].Vertex2.Y,
                            -bgf.MappingObject.VertexMappings[face.Face.A].Vertex2.Z),
                        TexCoord = new Vector2(face.TextureMapping.TextureCoordinates1.U,
                            face.TextureMapping.TextureCoordinates1.V)
                    }
                let vertex2 =
                    new GltfVertexData
                    {
                        Position = new Vector3(bgf.MappingObject.VertexMappings[face.Face.B].Vertex1.X,
                            bgf.MappingObject.VertexMappings[face.Face.B].Vertex1.Y,
                            -bgf.MappingObject.VertexMappings[face.Face.B].Vertex1.Z),
                        Normal = new Vector3(bgf.MappingObject.VertexMappings[face.Face.B].Vertex2.X,
                            bgf.MappingObject.VertexMappings[face.Face.B].Vertex2.Y,
                            -bgf.MappingObject.VertexMappings[face.Face.B].Vertex2.Z),
                        TexCoord = new Vector2(face.TextureMapping.TextureCoordinates2.U,
                            face.TextureMapping.TextureCoordinates2.V)
                    }
                let vertex3 = new GltfVertexData
                {
                    Position = new Vector3(bgf.MappingObject.VertexMappings[face.Face.C].Vertex1.X,
                        bgf.MappingObject.VertexMappings[face.Face.C].Vertex1.Y,
                        -bgf.MappingObject.VertexMappings[face.Face.C].Vertex1.Z),
                    Normal = new Vector3(bgf.MappingObject.VertexMappings[face.Face.C].Vertex2.X,
                        bgf.MappingObject.VertexMappings[face.Face.C].Vertex2.Y,
                        -bgf.MappingObject.VertexMappings[face.Face.C].Vertex2.Z),
                    TexCoord = new Vector2(face.TextureMapping.TextureCoordinates3.U,
                        face.TextureMapping.TextureCoordinates3.V)
                }
                select new GltfTriangleData { Vertex1 = vertex1, Vertex2 = vertex2, Vertex3 = vertex3 }).ToArray(),
            IsTransparentTexture = isTransparentTexture
        };
    }

    internal static string? GetTexturePath(string textureName, Dictionary<string, string> extractedTextures,
        TxsStruct? txs)
    {
        while (true)
        {
            foreach (var (extractedTextureName, extractedTexturePath) in extractedTextures)
                if (extractedTextureName.NormalizeName() == textureName.NormalizeName())
                    return extractedTexturePath;

            if (txs is null) return null;
            textureName = txs.TextureNames[0];
            txs = null;
        }
    }

    internal static SceneBuilder CreateModel(GltfModelData modelData)
    {
        var model = ModelRoot.CreateModel();
        model.Asset.Generator = "Europa1400.Tools";
        model.Asset.Copyright = "europa1400-community";

        var scene = new SceneBuilder();
        var node = new NodeBuilder();
        var mesh = new MeshBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>(modelData.Name);

        scene.AddRigidMesh(mesh, node);

        for (var primitiveIndex = 0; primitiveIndex < modelData.Primitives.Length; primitiveIndex++)
        {
            var primitiveData = modelData.Primitives[primitiveIndex];

            var memoryImage = new MemoryImage(primitiveData.TexturePath);
            var image = ImageBuilder.From(memoryImage, primitiveData.TextureName);
            var material = new MaterialBuilder(primitiveData.TextureName)
                .WithBaseColor(image)
                .WithDoubleSide(true)
                .WithAlpha(primitiveData.IsTransparentTexture ? AlphaMode.OPAQUE : AlphaMode.OPAQUE);

            var primitive = mesh.UsePrimitive(material);

            for (var triangleIndex = 0; triangleIndex < primitiveData.Triangles.Length; triangleIndex++)
            {
                var triangle = primitiveData.Triangles[triangleIndex];

                var vertex1 = new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>();
                var vertex2 = new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>();
                var vertex3 = new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>();

                vertex1.Geometry.Position = triangle.Vertex1.Position;
                vertex2.Geometry.Position = triangle.Vertex2.Position;
                vertex3.Geometry.Position = triangle.Vertex3.Position;

                vertex1.Geometry.Normal = triangle.Vertex1.Normal;
                vertex2.Geometry.Normal = triangle.Vertex2.Normal;
                vertex3.Geometry.Normal = triangle.Vertex3.Normal;

                vertex1.Material.TexCoord = triangle.Vertex1.TexCoord;
                vertex2.Material.TexCoord = triangle.Vertex2.TexCoord;
                vertex3.Material.TexCoord = triangle.Vertex3.TexCoord;

                primitive.AddTriangle(
                    vertex1,
                    vertex2,
                    vertex3
                );
            }
        }

        return scene;
    }
}