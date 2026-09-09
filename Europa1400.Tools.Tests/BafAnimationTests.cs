using System.IO.Compression;
using System.Numerics;
using System.Text;
using Europa1400.Tools.Animation;
using Europa1400.Tools.Pipeline.Converter;
using Europa1400.Tools.Structs;
using Europa1400.Tools.Structs.Baf;
using Europa1400.Tools.Structs.Bgf;
using ImageMagick;
using SharpGLTF.Schema2;

namespace Europa1400.Tools.Tests;

public class BafAnimationTests
{
    [Fact]
    public void Decoder_GroupsObjectsPerFrame_AndReadsMotionAndAttachments()
    {
        using var reader = new BinaryReader(new MemoryStream(WriteBaf([2, 1], true)));
        var baf = BafStruct.FromBytes(reader);
        Assert.Equal(2, baf.Frames.Length);
        Assert.Equal(2u, baf.ObjectCount);
        Assert.Equal(3u, baf.VertexCount);
        Assert.Equal(new[] { 2, 1 }, baf.Frames[1].ObjectVertexCounts);
        Assert.Equal(2u, baf.Frames[1].KeyValue);
        Assert.Equal(12f, baf.Frames[1].Positions[2].X);
        Assert.Equal(7f, baf.Frames[1].MotionPosition!.X);
        var anchor = Assert.Single(baf.Frames[1].Attachments);
        Assert.Equal("d3_Hand", anchor.Name);
        Assert.Equal(0x3A, anchor.Unknown1);
        Assert.Equal(4f, anchor.Vector2.X);
        Assert.Equal(0, baf.LoopIn);
        Assert.Equal(1, baf.LoopOut);
        Assert.Equal(reader.BaseStream.Length, reader.BaseStream.Position);
    }

    [Fact]
    public void Decoder_OldHeader_InfersSingleObjectAndVertexCount()
    {
        using var reader = new BinaryReader(new MemoryStream(WriteBaf([3], false)));
        var baf = BafStruct.FromBytes(reader);
        Assert.Equal(1u, baf.ObjectCount);
        Assert.Equal(3u, baf.VertexCount);
        Assert.Equal(2, baf.Frames.Length);
        Assert.All(baf.Frames, f => Assert.Null(f.MotionPosition));
    }

    [Fact]
    public void Decoder_TruncatedPayloads_FailInsteadOfReturningPartialFrames()
    {
        var bytes = WriteBaf([2, 1], true);
        for (var length = 0; length < bytes.Length; length++)
        {
            using var reader = new BinaryReader(new MemoryStream(bytes[..length]));
            Assert.ThrowsAny<IOException>(() => BafStruct.FromBytes(reader));
        }
    }

    [Fact]
    public void MorphGlb_PreservesVariantRestShape_AndSeparatesCoincidentSourceVertices()
    {
        var directory = Directory.CreateTempSubdirectory("baf-morph-test-");
        try
        {
            var texture = Path.Combine(directory.FullName, "body.png");
            using (var image = new MagickImage(MagickColors.White, 2, 2)) image.Write(texture);
            var bgf = CreateModel();
            var baf = CreateAnimation();
            var root = BgfAnimationConverter.Convert(bgf, baf, "walk",
                new Dictionary<string, string> { ["body"] = texture });
            var path = Path.Combine(directory.FullName, "animation.glb");
            root.SaveGLB(path);
            root = ModelRoot.Load(path);
            var animation = Assert.Single(root.LogicalAnimations);
            Assert.Equal(.14f, animation.Duration, 5);
            var node = Assert.Single(root.LogicalNodes.Where(n => n.Mesh != null));
            var primitive = Assert.Single(node.Mesh.Primitives);
            Assert.Equal(3, primitive.MorphTargetsCount);
            Assert.Equal(6, primitive.GetVertexAccessor("POSITION").Count);
            var positions = primitive.GetVertexAccessor("POSITION").AsVector3Array();
            Assert.Equal(positions[0], positions[3]);
            var deltas = primitive.GetMorphTargetAccessors(1)["POSITION"].AsVector3Array();
            Assert.Equal(Vector3.Zero, deltas[0]);
            Assert.Equal(new Vector3(0, 0, -2), deltas[3]);

            // Nonuniform source keys 10, 12, 20: halfway to key 12 is at 14 ms.
            var samplers = node.GetCurveSamplers(animation);
            var weights = samplers.GetMorphingWeights<float[]>(.014f);
            Assert.Equal(.5f, weights[0], 5);
            Assert.Equal(.5f, weights[1], 5);
            Assert.Equal(0f, weights[2], 5);
            var world = node.GetWorldMatrix(animation, .014f);
            var a = Vector3.Transform(positions[0] + deltas[0] * weights[1], world);
            var b = Vector3.Transform(positions[3] + deltas[3] * weights[1], world);
            Assert.Equal(1.5f, a.X, 5);
            Assert.Equal(0f, a.Z, 5);
            Assert.Equal(-1f, b.Z, 5);
            Assert.Equal(.028f, animation.Extras!["loopStart"]!.GetValue<float>(), 5);
            Assert.Equal(.14f, animation.Extras!["loopEnd"]!.GetValue<float>(), 5);
        }
        finally { directory.Delete(true); }
    }

    [Fact]
    public void MorphGlb_KeepsCollapsedTrianglesAndRepairsNormals_WhenTextureIsMissing()
    {
        var bgf = CreateModel();
        foreach (var vertex in bgf.MappingObject.VertexMappings)
        {
            vertex.Vertex1 = new();
            vertex.Vertex2 = new() { Z = 8 };
        }
        var root = BgfAnimationConverter.Convert(bgf, CreateAnimation(), "unfold", []);
        var p = Assert.Single(Assert.Single(root.LogicalMeshes).Primitives);
        Assert.Equal(2, p.GetTriangleIndices().Count());
        Assert.Equal(3, p.MorphTargetsCount);
        Assert.All(p.GetVertexAccessor("NORMAL").AsVector3Array(), n => Assert.Equal(1f, n.Length(), 5));
        Assert.Equal("body.bmp", p.Material.Extras!["missingTexture"]!.GetValue<string>());
        // Validate the complete GLB despite all base triangles having zero area.
        var path = Path.Combine(Path.GetTempPath(), $"baf-unfold-{Guid.NewGuid():N}.glb");
        try { root.SaveGLB(path); ModelRoot.Load(path); }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Conversion_RejectsSameTotalButDifferentObjectLayout()
    {
        var bgf = CreateModel();
        var baf = CreateAnimation([2, 2]);
        Assert.False(BgfAnimationCatalog.HasCompatibleLayout(bgf, baf));
        Assert.Throws<InvalidDataException>(() =>
            BgfAnimationConverter.Convert(bgf, baf, "wrong", []));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BgfAnimationConverter.Convert(bgf, CreateAnimation(), "wrong", [], keyTicksPerSecond: 0));
    }

    [Fact]
    public void Catalog_RequiresFamilyDetailAndNumberedVariant()
    {
        var directory = Directory.CreateTempSubdirectory("baf-catalog-test-");
        try
        {
            foreach (var relative in new[] { "Character/Mann/gehen_MANN.baf", "Character/Frau/gehen_FRAU.baf",
                         "LowPolyCharacter/Mann/gehen_MANN_LOW.baf", "Vegetation/faellen_BUCHE_01.baf",
                         "Vegetation/faellen_BUCHE_02.baf", "Vegetation/faellen_EICHE_01.baf" })
            {
                var path = Path.Combine(directory.FullName, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, WriteBaf([4], false));
            }
            var catalog = BgfAnimationCatalog.FromDirectory(directory.FullName);
            var bgf = CreateModel();
            Assert.Equal("gehen_MANN", Assert.Single(catalog.FindForModel(bgf,
                "_DYNAMIC/Character/Arbeiter_MANN.bgf")).Name);
            Assert.Equal("gehen_MANN_LOW", Assert.Single(catalog.FindForModel(bgf,
                "_DYNAMIC/Low_Poly_Character/Arbeiter_Low_MANN.bgf")).Name);
            Assert.Equal("faellen_BUCHE_01", Assert.Single(catalog.FindForModel(bgf,
                "Vegetation/BUCHE/pfl_R_BUCHE_01_FAELLT.bgf")).Name);
            Assert.Empty(catalog.FindForModel(bgf, "Furniture/Unknown.bgf"));
        }
        finally { directory.Delete(true); }
    }

    [Fact]
    public void AllGameAnimations_DecodeCompletelyWithStableLayouts()
    {
        using var archive = ZipFile.OpenRead(Path.Combine(EnvVariables.GameDirectoryPath, "Resources", "animations.bin"));
        var entries = archive.Entries.Where(e => e.FullName.EndsWith(".baf", StringComparison.OrdinalIgnoreCase)).ToArray();
        Assert.NotEmpty(entries);
        foreach (var entry in entries)
        {
            using var source = entry.Open();
            using var stream = new MemoryStream();
            source.CopyTo(stream);
            stream.Position = 0;
            using var reader = new BinaryReader(stream);
            var baf = BafStruct.FromBytes(reader);
            Assert.True(stream.Position == stream.Length, entry.FullName);
            Assert.All(baf.Frames, frame =>
            {
                Assert.Equal(baf.VertexCount, (uint)frame.Positions.Length);
                Assert.Equal(baf.ObjectCount, (uint)frame.ObjectVertexCounts.Length);
                Assert.Equal(baf.AttachmentCount, (uint)frame.Attachments.Length);
            });
        }
    }

    private static byte[] WriteBaf(int[] layout, bool modern)
    {
        using var stream = new MemoryStream();
        using var w = new BinaryWriter(stream, Encoding.Latin1, true);
        w.Write(Encoding.Latin1.GetBytes("BGF\0"));
        w.Write((byte)0x30); w.Write(0u);
        w.Write(new byte[] { 1, 1, 0, 0xCD, 0xAB, 0x23 }); w.Write(2u);
        if (modern)
        {
            w.Write(new byte[] { 0x33, 1, 0x24, 0, 0x37 }); w.Write(1u);
            w.Write((byte)0x36); w.Write((uint)layout.Length);
            w.Write((byte)0x34); w.Write((uint)layout.Sum());
            w.Write((byte)0x29); w.Write(0);
            w.Write((byte)0x2A); w.Write(1);
        }
        for (var frame = 0; frame < 2; frame++)
        {
            var vertex = 0;
            foreach (var count in layout)
            {
                w.Write((byte)0x18); w.Write((uint)(frame * 2));
                w.Write((byte)0x19); w.Write((uint)count); w.Write((byte)0x21);
                for (var v = 0; v < count; v++) WriteVector(w, frame * 10 + vertex++, 1, 2);
                w.Write((byte)0x28);
            }
            if (modern)
            {
                w.Write((byte)0x31); WriteVector(w, 7, 8, 9); WriteVector(w, 1, 2, 3);
                w.Write((byte)0x38); w.Write(Encoding.Latin1.GetBytes("d3_Hand\0"));
                w.Write((byte)0x39); WriteVector(w, 1, 2, 3);
                w.Write((byte)0x3A); WriteVector(w, 4, 5, 6);
            }
        }
        w.Write((byte)0x2F);
        stream.Position = 5; w.Write((uint)(stream.Length - 10));
        return stream.ToArray();
    }

    private static void WriteVector(BinaryWriter w, float x, float y, float z)
    { w.Write(x); w.Write(y); w.Write(z); }

    private static BafStruct CreateAnimation(int[]? layout = null) => new()
    {
        Magic = "BGF", DataSize = 0, BoneCount = 4, ObjectCount = (uint)(layout?.Length ?? 1),
        LoopIn = 1, LoopOut = 2,
        Frames = Enumerable.Range(0, 3).Select(f => new BafFrame
        {
            KeyValue = new uint[] { 10, 12, 20 }[f],
            ObjectVertexCounts = layout ?? [4],
            Positions = Enumerable.Range(0, 4).Select(v => new Vector3Struct
            { X = 100 + f * 3, Y = 100, Z = v == 3 ? f * 2 : 0 }).ToArray(),
            MotionPosition = new Vector3Struct { X = 100 + f * 3, Y = 10 }
        }).ToArray()
    };

    private static BgfStruct CreateModel() => new()
    {
        Header = new BgfHeaderStruct { Name = "morph-regression" },
        Textures = [new() { Name = "body.bmp" }],
        Footer = new BgfFooterStruct { TextureNames = [new() { Name = "body" }] },
        GameObjects = [new() { Model = new BgfModelStruct { Vertices = [new(), new(), new(), new()], Polygons = [] } }],
        MappingObject = new BgfMappingObjectStruct
        {
            TextureCount = 1, VertexMappingCount = 4, PolygonMappingCount = 2,
            VertexMappings = [Vertex(0, 0), Vertex(2, 0), Vertex(0, 2), Vertex(0, 0)],
            PolygonMappings = new uint[] { 0, 3 }.Select(a => new BgfPolygonMappingStruct
            {
                Face = new() { A = a, B = 1, C = 2 }, TextureIndex = 0,
                TextureMapping = new() { ValuesU = new() { X = 1, Y = 2, Z = 1 },
                    ValuesV = new() { X = 0, Y = 0, Z = 1 }, ValuesW = new() }
            }).ToArray()
        }
    };

    private static BgfVertexMapping Vertex(float x, float y) => new()
    { Vertex1 = new() { X = x, Y = y }, Vertex2 = new() { Z = -1 } };
}
