using System.IO;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Windows.Media.Media3D;
using Europa1400.Tools.Verifier;
using SharpGLTF.Schema2;
using Xunit;

namespace Europa1400.Tools.Verifier.Tests;

public class GltfAnimationPreviewTests
{
    [Fact]
    public void ReloadedGlb_SamplesMorphsRootMotionAndNormals_WithoutChangingUvs()
    {
        var directory = Directory.CreateTempSubdirectory("verifier-animation-");
        try
        {
            var root = ModelRoot.CreateModel();
            var node = root.UseScene("scene").CreateNode("moving mesh");
            node.Mesh = root.CreateMesh();
            node.Extras = new JsonObject { ["bgfRootMotion"] = true };
            var p = node.Mesh.CreatePrimitive();
            p.WithVertexAccessor("POSITION", new[] { Vector3.Zero, Vector3.UnitX, Vector3.UnitY });
            p.WithVertexAccessor("NORMAL", Enumerable.Repeat(Vector3.UnitZ, 3).ToArray());
            p.WithVertexAccessor("TEXCOORD_0", new[] { Vector2.Zero, Vector2.UnitX, Vector2.UnitY });
            p.WithIndicesAccessor(PrimitiveType.TRIANGLES, new[] { 0, 1, 2 });
            var view = root.CreateBufferView(36, 0, BufferMode.ARRAY_BUFFER);
            var accessor = root.CreateAccessor();
            accessor.SetVertexData(view, 0, 3, DimensionType.VEC3, EncodingType.FLOAT, false);
            var deltas = accessor.AsVector3Array();
            for (var i = 0; i < 3; i++) deltas[i] = Vector3.UnitZ * 2;
            accessor.UpdateBounds();
            p.SetMorphTargetAccessors(0, new Dictionary<string, Accessor> { ["POSITION"] = accessor });
            var clip = root.CreateAnimation("test");
            clip.CreateMorphChannel(node, new Dictionary<float, float[]> { [0] = [0], [1] = [1] }, 1, true);
            clip.CreateTranslationChannel(node, new Dictionary<float, Vector3>
            { [0] = Vector3.Zero, [1] = new Vector3(10, 0, 0) }, true);
            clip.Extras = new JsonObject { ["loopStart"] = .25f, ["loopEnd"] = .75f };
            var path = Path.Combine(directory.FullName, "test.glb");
            root.SaveGLB(path);

            RunSta(() =>
            {
                var preview = ModelPreviewLoader.LoadAnimated(path);
                var mesh = (MeshGeometry3D)((GeometryModel3D)Assert.Single(preview.Model.Children)).Geometry;
                Assert.False(mesh.IsFrozen);
                Assert.Single(preview.Animations);
                Assert.Equal((.25f, .75f), preview.LoopRange(0));
                var uvs = mesh.TextureCoordinates.ToArray();
                preview.SetTime(0, .5f, false);
                Assert.Equal(new Point3D(5, 0, 1), mesh.Positions[0]);
                Assert.All(mesh.Normals, n => Assert.Equal(new Vector3D(0, 0, 1), n));
                preview.SetTime(0, .5f, true);
                Assert.Equal(new Point3D(0, 0, 1), mesh.Positions[0]);
                Assert.Equal(uvs, mesh.TextureCoordinates.ToArray());
                preview.SetTime(0, 0, true);
                Assert.Equal(new Point3D(0, 0, 0), mesh.Positions[0]);
                preview.SetTime(0, 10, false);
                Assert.Equal(new Point3D(10, 0, 2), mesh.Positions[0]);
            });
        }
        finally { directory.Delete(true); }
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() => { try { action(); } catch (Exception ex) { failure = ex; } });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
