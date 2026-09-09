using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Threading;
using Europa1400.Tools.Animation;
using Europa1400.Tools.Gltf;
using Europa1400.Tools.Structs;
using Europa1400.Tools.Structs.Baf;
using Europa1400.Tools.Structs.Bgf;
using Europa1400.Tools.Structs.Txs;
using SharpGLTF.Schema2;

namespace Europa1400.Tools.Pipeline.Converter;

/// <summary>Exports one BAF clip as standard glTF morph animation with its original vertex bindings.</summary>
public static class BgfAnimationConverter
{
    // GildeGold.exe 0x531C03: 14 ms timer; 0x432969 advances keys by elapsed ticks.
    public const float DefaultKeyTicksPerSecond = 1000f / 14;

    public static ModelRoot Convert(BgfStruct bgf, BafStruct baf, string animationName,
        Dictionary<string, string> textures, TxsStruct? txs = null, float keyTicksPerSecond = DefaultKeyTicksPerSecond,
        CancellationToken cancellationToken = default)
    {
        if (!float.IsFinite(keyTicksPerSecond) || keyTicksPerSecond <= 0)
            throw new ArgumentOutOfRangeException(nameof(keyTicksPerSecond));
        if (!BgfAnimationCatalog.HasCompatibleLayout(bgf, baf))
            throw new InvalidDataException("The BAF object/vertex layout does not match this BGF.");

        var data = GltfUtil.GetModelData(bgf, null, txs, textures, includeMissingTextures: true);
        if (data.Primitives.Length == 0)
            throw new InvalidDataException("The BGF has no textured visual faces to animate.");
        var root = ModelRoot.CreateModel();
        root.Asset.Generator = "Europa1400.Tools";
        var meshNode = root.UseScene(data.Name).CreateNode();
        var mesh = root.CreateMesh(data.Name);
        meshNode.Mesh = mesh;
        var count = baf.Frames.Length;
        var basis = bgf.MappingObject.VertexMappings.Select(v => Export(v.Vertex1)).ToArray();
        var first = baf.Frames[0];
        var keyframes = new Dictionary<float, float[]>();
        var rootMotion = new Dictionary<float, Vector3>();
        var firstMotion = first.MotionPosition == null ? Vector3.Zero : Export(first.MotionPosition);
        var positions = new Vector3[count][];
        var normals = new Vector3[count][];
        var faces = BgfVisualGeometry.GetFaces(bgf);
        for (var f = 0; f < count; f++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var frame = baf.Frames[f];
            var time = (frame.KeyValue - first.KeyValue) / keyTicksPerSecond;
            var motion = frame.MotionPosition == null ? Vector3.Zero : Export(frame.MotionPosition) - firstMotion;
            motion.Y = 0;
            rootMotion.Add(time, motion);
            var weights = new float[count];
            weights[f] = 1;
            keyframes.Add(time, weights);
            var pose = new Vector3[basis.Length];
            for (var v = 0; v < pose.Length; v++)
            {
                // GildeGold.exe 0x44FD39: frame N minus frame 0, applied to the selected BGF.
                pose[v] = basis[v] + Export(frame.Positions[v]) - Export(first.Positions[v]) - motion;
            }
            positions[f] = pose;
            var n = new Vector3[basis.Length];
            foreach (var face in faces)
            {
                var normal = Vector3.Cross(pose[face.B] - pose[face.A], pose[face.C] - pose[face.A]);
                n[face.A] += normal; n[face.B] += normal; n[face.C] += normal;
            }
            for (var v = 0; v < n.Length; v++)
                n[v] = n[v].LengthSquared() > 1e-12f ? Vector3.Normalize(n[v]) :
                    SafeNormal(Export(bgf.MappingObject.VertexMappings[v].Vertex2));
            normals[f] = n;
        }

        foreach (var source in data.Primitives)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Static MeshBuilder drops triangles that collapse in the rest pose;
            // some BAFs unfold those triangles into visible geometry.
            var primitive = mesh.CreatePrimitive();
            primitive.Material = root.CreateMaterial(GltfUtil.CreateMaterial(source));
            if (source.TexturePath == null)
                primitive.Material.Extras = new JsonObject { ["missingTexture"] = source.TextureName };
            // Different source vertices can share a rest position and diverge during animation.
            var vertices = source.Triangles.SelectMany(t => new[] { t.Vertex1, t.Vertex2, t.Vertex3 }).ToArray();
            var basePositions = vertices.Select(v => v.Position).ToArray();
            var baseNormals = vertices.Select(v => SafeNormal(v.Normal)).ToArray();
            primitive.WithVertexAccessor("POSITION", basePositions);
            primitive.WithVertexAccessor("NORMAL", baseNormals);
            primitive.WithVertexAccessor("TEXCOORD_0", vertices.Select(v => v.TexCoord).ToArray());
            primitive.WithIndicesAccessor(PrimitiveType.TRIANGLES, Enumerable.Range(0, vertices.Length).ToArray());
            for (var f = 0; f < count; f++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var positionDeltas = new Vector3[vertices.Length];
                var normalDeltas = new Vector3[vertices.Length];
                for (var v = 0; v < vertices.Length; v++)
                {
                    var index = vertices[v].SourceVertexIndex;
                    positionDeltas[v] = positions[f][index] - basePositions[v];
                    normalDeltas[v] = normals[f][index] - baseNormals[v];
                }
                primitive.SetMorphTargetAccessors(f, new Dictionary<string, Accessor>
                {
                    ["POSITION"] = CreateAccessor(root, positionDeltas),
                    ["NORMAL"] = CreateAccessor(root, normalDeltas)
                });
            }
        }

        var animation = root.CreateAnimation(animationName);
        animation.CreateMorphChannel(meshNode, keyframes, count, true);
        animation.CreateTranslationChannel(meshNode, rootMotion, true);
        meshNode.Name = "BGF animated mesh";
        meshNode.Extras = new JsonObject { ["bgfRootMotion"] = true };
        var loopIn = baf.LoopIn >= 0 && baf.LoopIn < count ? baf.LoopIn : 0;
        var loopOut = baf.LoopOut > loopIn && baf.LoopOut < count ? baf.LoopOut : count - 1;
        animation.Extras = new JsonObject
        {
            ["loopStart"] = (baf.Frames[loopIn].KeyValue - first.KeyValue) / keyTicksPerSecond,
            ["loopEnd"] = (baf.Frames[loopOut].KeyValue - first.KeyValue) / keyTicksPerSecond,
            ["sourceKeyTicksPerSecond"] = keyTicksPerSecond
        };
        return root;
    }

    private static Accessor CreateAccessor(ModelRoot root, Vector3[] values)
    {
        var view = root.CreateBufferView(checked(values.Length * 12), 0, BufferMode.ARRAY_BUFFER);
        var accessor = root.CreateAccessor();
        accessor.SetVertexData(view, 0, values.Length, DimensionType.VEC3, EncodingType.FLOAT, false);
        var target = accessor.AsVector3Array();
        for (var i = 0; i < values.Length; i++) target[i] = values[i];
        accessor.UpdateBounds();
        return accessor;
    }

    private static Vector3 SafeNormal(Vector3 v) => float.IsFinite(v.LengthSquared()) && v.LengthSquared() > 1e-12f ? Vector3.Normalize(v) : Vector3.UnitY;
    private static Vector3 Export(Vector3Struct v) => new(v.X, v.Y, -v.Z);
}
