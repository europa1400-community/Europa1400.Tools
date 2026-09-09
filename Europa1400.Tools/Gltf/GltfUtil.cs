using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Europa1400.Tools.Structs.Baf;
using Europa1400.Tools.Structs.Txs;
using Europa1400.Tools.Extensions;
using Europa1400.Tools.Structs.Bgf;
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
    internal static GltfModelData GetModelData(BgfStruct bgf, BafStruct[]? bafs, TxsStruct? txs,
        Dictionary<string, string> extractedTextures, bool includeMissingTextures = false)
    {
        var primitives = bgf.Textures
            .Select((texture, textureIndex) => GetPrimitiveData(bgf, texture, textureIndex, extractedTextures, txs, includeMissingTextures))
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
        int textureIndex, Dictionary<string, string> extractedTextures, TxsStruct? txs, bool includeMissingTextures)
    {
        var textureName = texture.Name;
        var isTransparentTexture = texture.HasTransparency;
        var texturePath = GetTexturePath(textureName, extractedTextures, txs);

        if (texturePath is null && !includeMissingTextures) return null;

        var faces = BgfVisualGeometry.GetFaces(bgf)
            .Where(e => e.TextureIndex == textureIndex)
            .ToArray();

        if (faces.Length == 0) return null;

        return new GltfPrimitiveData
        {
            TextureName = textureName,
            TexturePath = texturePath,
            Triangles = (from face in faces
                let vertex1 = CreateVertex(bgf, face.A, face.Uv1)
                let vertex2 = CreateVertex(bgf, face.B, face.Uv2)
                let vertex3 = CreateVertex(bgf, face.C, face.Uv3)
                select new GltfTriangleData { Vertex1 = vertex1, Vertex2 = vertex2, Vertex3 = vertex3 }).ToArray(),
            IsTransparentTexture = isTransparentTexture
        };
    }

    private static GltfVertexData CreateVertex(BgfStruct bgf, uint index, Vector2 uv)
    {
        var mapping = bgf.MappingObject.VertexMappings[(int)index];
        var position = BgfVertexMapping.ToExport(mapping.Vertex1);
        var normal = BgfVertexMapping.ToExport(mapping.Vertex2);
        return new GltfVertexData
        {
            SourceVertexIndex = index,
            Position = new Vector3(position.X, position.Y, position.Z),
            Normal = new Vector3(normal.X, normal.Y, normal.Z),
            TexCoord = uv
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

    internal static MaterialBuilder CreateMaterial(GltfPrimitiveData primitive)
    {
        var material = new MaterialBuilder(primitive.TextureName)
            .WithDoubleSide(true)
            .WithAlpha(AlphaMode.OPAQUE);
        if (primitive.TexturePath != null)
            material.WithBaseColor(ImageBuilder.From(new MemoryImage(primitive.TexturePath), primitive.TextureName));
        else
            material.WithBaseColor(new Vector4(.8f, .8f, .8f, 1));
        return material;
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

            var material = CreateMaterial(primitiveData);

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