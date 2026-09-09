using System.Numerics;
using System.Text;
using Europa1400.Tools.Gltf;
using Europa1400.Tools.Pipeline.Converter;
using Europa1400.Tools.Structs;
using Europa1400.Tools.Structs.Bgf;
using ImageMagick;
using SharpGLTF.Schema2;

namespace Europa1400.Tools.Tests;

public class BgfTextureMappingTests
{
    [Fact]
    public void Footer_ReadsThreeStringsAndFixedMetadata_StopsBeforeAttachments()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.Latin1, true))
        {
            WriteTexture(writer, "body", "", "Kiste", [0, 1, 0xCB, 0, 0x2F, 0]);
            WriteTexture(writer, "", "reflekt1", "", [1, 0xAF, 2, 0, 0, 0]);
            writer.Write(1u); // Attachment count, not a texture name.
            writer.Write(Encoding.Latin1.GetBytes("d3_Head\0"));
        }
        stream.Position = 0;
        using var reader = new BinaryReader(stream);
        var footer = BgfFooterStruct.FromBytes(reader, 2);

        Assert.Equal(2, footer.TextureNames.Length);
        Assert.Equal("body", footer.TextureNames[0].Name);
        Assert.Equal("Kiste", footer.TextureNames[0].NameAppendix);
        Assert.Equal(new byte[] { 0, 1, 0xCB, 0, 0x2F, 0 }, footer.TextureNames[0].Metadata);
        Assert.Equal("", footer.TextureNames[1].Name);
        Assert.Equal("reflekt1", footer.TextureNames[1].ResolvedName);
        Assert.Equal(1u, reader.ReadUInt32());
    }

    [Fact]
    public void Footer_ZeroTextures_DoesNotConsumeAttachments()
    {
        using var reader = new BinaryReader(new MemoryStream([3, 0, 0, 0]));
        Assert.Empty(BgfFooterStruct.FromBytes(reader, 0).TextureNames);
        Assert.Equal(0, reader.BaseStream.Position);
    }

    [Fact]
    public void Footer_TruncatedMetadata_Throws()
    {
        using var reader = new BinaryReader(new MemoryStream([0, 0, 0, 1, 2, 3, 4, 5]));
        Assert.Throws<EndOfStreamException>(() => BgfFooterStruct.FromBytes(reader, 1));
    }

    [Fact]
    public void VisualFaces_UseRuntimeUvsAndFooterMaterial_EvenWhenEditorCountsMatch()
    {
        var bgf = CreateModel();
        var face = Assert.Single(BgfVisualGeometry.GetFaces(bgf));
        Assert.Equal(1, face.TextureIndex); // Footer body is file texture 1, not 0.
        Assert.Equal(new Vector2(.25f, .125f), face.Uv1);
        Assert.Equal(new Vector2(1.5f, -.25f), face.Uv2);
        Assert.Equal(new Vector2(-.5f, 1.25f), face.Uv3);

        bgf.Footer.TextureNames[0] = new BgfTextureNameStruct { Name = "", SecondaryName = "body" };
        Assert.Equal(1, Assert.Single(BgfVisualGeometry.GetFaces(bgf)).TextureIndex);
    }

    [Theory]
    [InlineData(255)]
    [InlineData(2)]
    public void VisualFaces_InvalidMaterial_DoesNotPaintWithTextureZero(byte index)
    {
        var bgf = CreateModel();
        bgf.MappingObject.PolygonMappings[0].TextureIndex = index;
        Assert.Equal(-1, Assert.Single(BgfVisualGeometry.GetFaces(bgf)).TextureIndex);
    }

    [Fact]
    public void GlbAndObj_PreserveCornerUvs_UsingTheirRespectiveImageOrigins()
    {
        var directory = Directory.CreateTempSubdirectory("bgf-uv-test-");
        try
        {
            var texturePath = Path.Combine(directory.FullName, "body.png");
            using (var image = new MagickImage(MagickColors.White, 2, 2))
                image.Write(texturePath);
            var textures = new Dictionary<string, string> { ["body"] = texturePath };
            var bgf = CreateModel();
            var data = GltfUtil.GetModelData(bgf, null, null, textures);
            Assert.Equal("body.bmp", Assert.Single(data.Primitives).TextureName);

            var path = Path.Combine(directory.FullName, "model.glb");
            GltfUtil.CreateModel(data).ToGltf2().SaveGLB(path);
            var glb = ModelRoot.Load(path);
            var primitive = Assert.Single(Assert.Single(glb.LogicalMeshes).Primitives);
            var uvs = primitive.GetVertexAccessor("TEXCOORD_0").AsVector2Array();
            var triangle = Assert.Single(primitive.GetTriangleIndices());
            Assert.Equal(new Vector2(.25f, .125f), uvs[triangle.A]);
            Assert.Equal(new Vector2(1.5f, -.25f), uvs[triangle.B]);
            Assert.Equal(new Vector2(-.5f, 1.25f), uvs[triangle.C]);

            new BgfWavefrontConverter { TargetDirectory = directory.FullName }
                .Convert(bgf, null, textures, "model");
            var obj = File.ReadAllLines(Path.Combine(directory.FullName, "model", "model.obj"));
            Assert.Equal(new[] { "vt 0.250000 0.875000", "vt 1.500000 1.250000", "vt -0.500000 -0.250000" },
                obj.Where(line => line.StartsWith("vt ")));
            Assert.Contains("usemtl body.bmp", obj);
        }
        finally
        {
            directory.Delete(true);
        }
    }

    private static void WriteTexture(BinaryWriter writer, string name, string secondary, string appendix, byte[] metadata)
    {
        writer.Write(Encoding.Latin1.GetBytes(name + "\0" + secondary + "\0" + appendix + "\0"));
        writer.Write(metadata);
    }

    private static BgfStruct CreateModel() => new()
    {
        Header = new BgfHeaderStruct { Name = "uv-regression" },
        Textures = [new() { Name = "head.bmp" }, new() { Name = "body.bmp" }],
        Footer = new BgfFooterStruct { TextureNames = [new() { Name = "body" }, new() { Name = "head" }] },
        GameObjects = [new() { Model = new BgfModelStruct
        {
            Vertices = [new(), new(), new()],
            Polygons = [new() { Face = new() { A = 0, B = 1, C = 2 }, TextureIndex = 0,
                TextureMapping = new() { ValuesU = new(), ValuesV = new(), ValuesW = new() } }]
        } }],
        MappingObject = new BgfMappingObjectStruct
        {
            TextureCount = 2, VertexMappingCount = 3, PolygonMappingCount = 1,
            VertexMappings = [
                Vertex(0, 0), Vertex(1, 0), Vertex(0, 1)
            ],
            PolygonMappings = [new()
            {
                Face = new() { A = 0, B = 1, C = 2 },
                TextureIndex = 0,
                TextureMapping = new()
                {
                    ValuesU = new() { X = 1.25f, Y = 2.5f, Z = .5f },
                    ValuesV = new() { X = .125f, Y = -.25f, Z = 1.25f },
                    ValuesW = new()
                }
            }]
        }
    };

    private static BgfVertexMapping Vertex(float x, float y) => new()
    {
        Vertex1 = new Vector3Struct { X = x, Y = y },
        Vertex2 = new Vector3Struct { Z = -1 }
    };
}
