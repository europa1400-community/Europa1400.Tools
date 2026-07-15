using System.Text;
using Europa1400.Tools.Font;
using Europa1400.Tools.Pipeline;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Converter;
using Europa1400.Tools.Pipeline.Decoder;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Gfx;
using Newtonsoft.Json;
using SkiaSharp;
using Xunit.Abstractions;

namespace Europa1400.Tools.Tests;

public class GlyphPackerTests
{
    [Fact]
    public void Pack_EmptyList_ReturnsZeroAtlas()
    {
        var (packed, atlasWidth, atlasHeight) = GlyphPacker.Pack([], []);

        Assert.Empty(packed);
        Assert.Equal(64, atlasWidth);
        Assert.Equal(64, atlasHeight);
    }

    [Fact]
    public void Pack_SingleGlyph_PlacedAtOrigin()
    {
        var (packed, atlasWidth, atlasHeight) = GlyphPacker.Pack([10], [20]);

        var single = Assert.Single(packed);
        Assert.Equal(0, single.Index);
        Assert.Equal(1, single.X);
        Assert.Equal(1, single.Y);
        Assert.Equal(10, single.Width);
        Assert.Equal(20, single.Height);
        Assert.True(atlasWidth >= 64);
        Assert.True(atlasHeight >= 64);
    }

    [Fact]
    public void Pack_SameSizeGlyphs_AllFitInAtlas()
    {
        var widths = Enumerable.Repeat(8, 100).ToArray();
        var heights = Enumerable.Repeat(12, 100).ToArray();

        var (packed, atlasWidth, atlasHeight) = GlyphPacker.Pack(widths, heights);

        Assert.Equal(100, packed.Length);
        Assert.Equal(100, packed.Select(p => p.Index).Distinct().Count());
        Assert.True(atlasWidth <= 4096);
        Assert.True(atlasHeight <= 4096);
    }

    [Fact]
    public void Pack_VariedSizeGlyphs_NoOverlap()
    {
        var random = new Random(42);
        var widths = Enumerable.Range(0, 50).Select(_ => random.Next(4, 32)).ToArray();
        var heights = Enumerable.Range(0, 50).Select(_ => random.Next(4, 32)).ToArray();

        var (packed, _, _) = GlyphPacker.Pack(widths, heights);

        Assert.Equal(50, packed.Length);
        for (var i = 0; i < packed.Length; i++)
        for (var j = i + 1; j < packed.Length; j++)
        {
            var a = packed[i];
            var b = packed[j];
            var noOverlap = a.X + a.Width <= b.X || b.X + b.Width <= a.X ||
                             a.Y + a.Height <= b.Y || b.Y + b.Height <= a.Y;
            Assert.True(noOverlap,
                $"Glyph {a.Index} ({a.X},{a.Y},{a.Width},{a.Height}) overlaps {b.Index} ({b.X},{b.Y},{b.Width},{b.Height})");
        }
    }

    [Fact]
    public void Pack_AllGlyphsWithinAtlasBounds()
    {
        var widths = new[] { 30, 45, 20, 55, 15, 40 };
        var heights = new[] { 25, 15, 35, 20, 30, 10 };

        var (packed, atlasWidth, atlasHeight) = GlyphPacker.Pack(widths, heights);

        foreach (var p in packed)
        {
            Assert.True(p.X + p.Width <= atlasWidth,
                $"Glyph {p.Index} right edge exceeds atlas width");
            Assert.True(p.Y + p.Height <= atlasHeight,
                $"Glyph {p.Index} bottom edge exceeds atlas height");
            Assert.True(p.X >= 0);
            Assert.True(p.Y >= 0);
        }
    }

    [Fact]
    public void Pack_PowerOfTwoAtlasSize()
    {
        var (_, atlasWidth, atlasHeight) = GlyphPacker.Pack([100, 200, 150], [100, 200, 150]);

        AssertPowerOfTwo(atlasWidth);
        AssertPowerOfTwo(atlasHeight);

        static void AssertPowerOfTwo(int v)
        {
            Assert.True(v > 0 && (v & (v - 1)) == 0, $"{v} is not a power of two");
        }
    }

    [Fact]
    public void Pack_AtLeast64Pixels()
    {
        var (_, atlasWidth, atlasHeight) = GlyphPacker.Pack([1], [1]);

        Assert.True(atlasWidth >= 64, $"atlasWidth {atlasWidth} < 64");
        Assert.True(atlasHeight >= 64, $"atlasHeight {atlasHeight} < 64");
    }

    [Fact]
    public void Pack_AtMost4096Pixels()
    {
        var widths = Enumerable.Repeat(512, 200).ToArray();
        var heights = Enumerable.Repeat(512, 200).ToArray();

        var (_, atlasWidth, atlasHeight) = GlyphPacker.Pack(widths, heights);

        Assert.True(atlasWidth <= 4096);
        Assert.True(atlasHeight <= 4096);
    }

    [Fact]
    public void Pack_PreservesAllGlyphIndices()
    {
        var widths = new[] { 10, 20, 30, 40, 50 };
        var heights = new[] { 5, 10, 15, 20, 25 };

        var (packed, _, _) = GlyphPacker.Pack(widths, heights);

        var indices = packed.Select(p => p.Index).OrderBy(i => i).ToArray();
        Assert.Equal(5, indices.Length);
        for (var i = 0; i < 5; i++)
            Assert.Equal(i, indices[i]);
    }

    [Fact]
    public void Pack_MaintainsCorrectDimensions()
    {
        var widths = new[] { 15, 25 };
        var heights = new[] { 10, 20 };

        var (packed, _, _) = GlyphPacker.Pack(widths, heights);

        var byIndex = packed.ToDictionary(p => p.Index);
        Assert.Equal(15, byIndex[0].Width);
        Assert.Equal(10, byIndex[0].Height);
        Assert.Equal(25, byIndex[1].Width);
        Assert.Equal(20, byIndex[1].Height);
    }
}

public class FontMetadataTests
{
    [Fact]
    public void Serialize_ValidMetadata_ProducesJson()
    {
        var metadata = new FontMetadata
        {
            Name = "_FONT_WHITE_9",
            AtlasWidth = 256,
            AtlasHeight = 128,
            LineHeight = 11,
            BaseLine = 9,
            Glyphs = new List<FontGlyphInfo>
            {
                new()
                {
                    CharCode = 32,
                    X = 2,
                    Y = 2,
                    Width = 3,
                    Height = 1,
                    Advance = 4,
                    OffsetX = 0,
                    OffsetY = -8
                },
                new()
                {
                    CharCode = 33,
                    X = 7,
                    Y = 2,
                    Width = 4,
                    Height = 9,
                    Advance = 5,
                    OffsetX = 0,
                    OffsetY = 0
                }
            }
        };

        var json = JsonConvert.SerializeObject(metadata, Formatting.None);

        Assert.Contains("\"name\":\"_FONT_WHITE_9\"", json);
        Assert.Contains("\"atlasWidth\":256", json);
        Assert.Contains("\"atlasHeight\":128", json);
        Assert.Contains("\"lineHeight\":11", json);
        Assert.Contains("\"baseLine\":9", json);
        Assert.Contains("\"charCode\":32", json);
        Assert.Contains("\"charCode\":33", json);
        Assert.Contains("\"advance\":4", json);
        Assert.Contains("\"advance\":5", json);
    }

    [Fact]
    public void Deserialize_RoundTrip_PreservesAllFields()
    {
        var original = new FontMetadata
        {
            Name = "_FONT_BLACK_12",
            AtlasWidth = 512,
            AtlasHeight = 256,
            LineHeight = 14,
            BaseLine = 12,
            Glyphs = new List<FontGlyphInfo>
            {
                new()
                {
                    CharCode = 65, X = 10, Y = 20, Width = 8, Height = 12,
                    Advance = 9, OffsetX = 1, OffsetY = -2
                }
            }
        };

        var json = JsonConvert.SerializeObject(original);
        var deserialized = JsonConvert.DeserializeObject<FontMetadata>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.AtlasWidth, deserialized.AtlasWidth);
        Assert.Equal(original.AtlasHeight, deserialized.AtlasHeight);
        Assert.Equal(original.LineHeight, deserialized.LineHeight);
        Assert.Equal(original.BaseLine, deserialized.BaseLine);
        Assert.Single(deserialized.Glyphs);

        var glyph = deserialized.Glyphs[0];
        Assert.Equal(65, glyph.CharCode);
        Assert.Equal(10, glyph.X);
        Assert.Equal(20, glyph.Y);
        Assert.Equal(8, glyph.Width);
        Assert.Equal(12, glyph.Height);
        Assert.Equal(9, glyph.Advance);
        Assert.Equal(1, glyph.OffsetX);
        Assert.Equal(-2, glyph.OffsetY);
    }

    [Fact]
    public void Serialize_EmptyGlyphs_HasEmptyArray()
    {
        var metadata = new FontMetadata
        {
            Name = "test",
            AtlasWidth = 64,
            AtlasHeight = 64,
            LineHeight = 10,
            BaseLine = 8,
            Glyphs = new List<FontGlyphInfo>()
        };

        var json = JsonConvert.SerializeObject(metadata, Formatting.None);

        Assert.Contains("\"glyphs\":[]", json);
    }
}

public class FontConversionTests
{
    [Fact]
    public async Task ConvertFontAsync_NullShapebank_ReturnsEmpty()
    {
        var converter = new GfxConverter();
        var input = new ShapebankDefinitionStruct
        {
            Name = "_FONT_TEST",
            Shapebank = null
        };

        var result = await converter.ConvertFontAsync(input);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ConvertFontAsync_NoGraphics_ReturnsEmpty()
    {
        var converter = new GfxConverter();
        var shapebank = new ShapebankStruct
        {
            GraphicsCount = 0,
            Graphics = [],
            UnknownData1 = []
        };
        var input = new ShapebankDefinitionStruct
        {
            Name = "_FONT_TEST",
            Shapebank = shapebank
        };

        var result = await converter.ConvertFontAsync(input);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ConvertFontAsync_OneGlyph_ReturnsPngAndJson()
    {
        var converter = new GfxConverter();
        var input = CreateFontDefinition("_FONT_TEST", [(4, 6)]);

        var result = (await converter.ConvertFontAsync(input)).ToList();

        Assert.Equal(2, result.Count);

        var pngExport = result.Single(e => e.FilePath.EndsWith(".png"));
        var jsonExport = result.Single(e => e.FilePath.EndsWith(".json"));

        Assert.Contains("_FONT_TEST", pngExport.FilePath);
        Assert.Contains("_FONT_TEST", jsonExport.FilePath);
        Assert.True(pngExport.Content.Length > 0);
        Assert.True(jsonExport.Content.Length > 0);

        var metadata = JsonConvert.DeserializeObject<FontMetadata>(
            Encoding.UTF8.GetString(jsonExport.Content));
        Assert.NotNull(metadata);
        Assert.Equal("_FONT_TEST", metadata.Name);
        Assert.Single(metadata.Glyphs);
        Assert.Equal(32, metadata.Glyphs[0].CharCode);
        Assert.Equal(4, metadata.Glyphs[0].Width);
        Assert.Equal(6, metadata.Glyphs[0].Height);
    }

    [Fact]
    public async Task ConvertFontAsync_ThreeGlyphs_AllInMetadata()
    {
        var converter = new GfxConverter();
        var input = CreateFontDefinition("_FONT_TEST", [(4, 6), (8, 10), (12, 14)]);

        var result = (await converter.ConvertFontAsync(input)).ToList();
        var jsonExport = result.Single(e => e.FilePath.EndsWith(".json"));
        var metadata = JsonConvert.DeserializeObject<FontMetadata>(
            Encoding.UTF8.GetString(jsonExport.Content));

        Assert.NotNull(metadata);
        Assert.Equal(3, metadata.Glyphs.Count);

        Assert.Equal(32, metadata.Glyphs[0].CharCode);
        Assert.Equal(33, metadata.Glyphs[1].CharCode);
        Assert.Equal(34, metadata.Glyphs[2].CharCode);

        Assert.Equal(4, metadata.Glyphs[0].Width);
        Assert.Equal(6, metadata.Glyphs[0].Height);
        Assert.Equal(8, metadata.Glyphs[1].Width);
        Assert.Equal(10, metadata.Glyphs[1].Height);
        Assert.Equal(12, metadata.Glyphs[2].Width);
        Assert.Equal(14, metadata.Glyphs[2].Height);
    }

    [Fact]
    public async Task ConvertFontAsync_AtlasPng_IsValidImage()
    {
        var converter = new GfxConverter();
        var input = CreateFontDefinition("_FONT_TEST", [(4, 6), (8, 10)]);

        var result = (await converter.ConvertFontAsync(input)).ToList();
        var pngExport = result.Single(e => e.FilePath.EndsWith(".png"));

        using var bitmap = SKBitmap.Decode(pngExport.Content);
        Assert.NotNull(bitmap);
        Assert.True(bitmap.Width > 0);
        Assert.True(bitmap.Height > 0);

        Assert.True(bitmap.Width >= 64);
        Assert.True(bitmap.Height >= 64);

        var isPowerOfTwo = (bitmap.Width & (bitmap.Width - 1)) == 0;
        var isPowerOfTwoH = (bitmap.Height & (bitmap.Height - 1)) == 0;
        Assert.True(isPowerOfTwo, $"Width {bitmap.Width} is not power of two");
        Assert.True(isPowerOfTwoH, $"Height {bitmap.Height} is not power of two");
    }

    [Fact]
    public async Task ConvertFontAsync_GlyphMetrics_AreCorrect()
    {
        var converter = new GfxConverter();
        var input = CreateFontDefinition("_FONT_TEST", [(10, 20)]);

        var result = (await converter.ConvertFontAsync(input)).ToList();
        var jsonExport = result.Single(e => e.FilePath.EndsWith(".json"));
        var metadata = JsonConvert.DeserializeObject<FontMetadata>(
            Encoding.UTF8.GetString(jsonExport.Content));

        Assert.NotNull(metadata);
        Assert.Equal(22, metadata.LineHeight); // 20 + 2
        Assert.Equal(20, metadata.BaseLine);
        Assert.Equal(20, metadata.Glyphs[0].Height);
        Assert.Equal(11, metadata.Glyphs[0].Advance); // 10 + 1
        Assert.Equal(0, metadata.Glyphs[0].OffsetX);
        Assert.Equal(0, metadata.Glyphs[0].OffsetY); // baseLine - height = 20 - 20 = 0
    }

    [Fact]
    public async Task ConvertFontAsync_MultipleGlyphSizes_VariedOffsets()
    {
        var converter = new GfxConverter();
        var input = CreateFontDefinition("_FONT_TEST", [(4, 8), (6, 12), (8, 10)]);

        var result = (await converter.ConvertFontAsync(input)).ToList();
        var jsonExport = result.Single(e => e.FilePath.EndsWith(".json"));
        var metadata = JsonConvert.DeserializeObject<FontMetadata>(
            Encoding.UTF8.GetString(jsonExport.Content));

        Assert.NotNull(metadata);
        Assert.Equal(14, metadata.LineHeight); // 12 + 2
        Assert.Equal(12, metadata.BaseLine);

        Assert.Equal(12 - 8, metadata.Glyphs[0].OffsetY);  // baseLine - height = 4
        Assert.Equal(12 - 12, metadata.Glyphs[1].OffsetY); // baseLine - height = 0
        Assert.Equal(12 - 10, metadata.Glyphs[2].OffsetY); // baseLine - height = 2
    }

    [Fact]
    public async Task ConvertFontAsync_OutputPaths_UseCorrectFormat()
    {
        var converter = new GfxConverter();
        var input = CreateFontDefinition("_FONT_WHITE_9", [(4, 6)]);

        var result = (await converter.ConvertFontAsync(input)).ToList();

        var pngPath = result.Single(e => e.FilePath.EndsWith(".png")).FilePath;
        var jsonPath = result.Single(e => e.FilePath.EndsWith(".json")).FilePath;

        Assert.Equal("fonts/_FONT_WHITE_9/_FONT_WHITE_9.png", pngPath);
        Assert.Equal("fonts/_FONT_WHITE_9/_FONT_WHITE_9.json", jsonPath);
    }

    [Fact]
    public void ConvertFontAsync_EstimateSteps_ReturnsGlyphCount()
    {
        var converter = new GfxConverter();
        var input = CreateFontDefinition("_FONT_TEST", [(4, 6), (8, 10), (12, 14)]);

        var steps = converter.EstimateSteps(input);

        Assert.Equal(3, steps);
    }

    [Fact]
    public async Task ConvertFontAsync_WithTransparentGlyphs_Succeeds()
    {
        var converter = new GfxConverter();

        var graphic = new GraphicStruct
        {
            Width = 3,
            Height = 3,
            Width2 = 3,
            Height2 = 3,
            Size = 200,
            SizeWithoutFooter = 100,
            GraphicRows =
            [
                new GraphicRowStruct
                {
                    BlockCount = 1,
                    TransparencyBlocks =
                    [
                        new TransparencyBlockStruct
                        {
                            Size = 6,
                            PixelCount = 1,
                            Data = [255, 0, 0]
                        }
                    ]
                },
                new GraphicRowStruct
                {
                    BlockCount = 1,
                    TransparencyBlocks =
                    [
                        new TransparencyBlockStruct
                        {
                            Size = 6,
                            PixelCount = 1,
                            Data = [0, 255, 0]
                        }
                    ]
                },
                new GraphicRowStruct
                {
                    BlockCount = 1,
                    TransparencyBlocks =
                    [
                        new TransparencyBlockStruct
                        {
                            Size = 6,
                            PixelCount = 1,
                            Data = [0, 0, 255]
                        }
                    ]
                }
            ]
        };

        var shapebank = new ShapebankStruct
        {
            GraphicsCount = 1,
            Graphics = [graphic],
            UnknownData1 = []
        };
        var input = new ShapebankDefinitionStruct
        {
            Name = "_FONT_TEST",
            Shapebank = shapebank
        };

        var result = (await converter.ConvertFontAsync(input)).ToList();

        Assert.Equal(2, result.Count);
        var pngExport = result.Single(e => e.FilePath.EndsWith(".png"));
        using var bitmap = SKBitmap.Decode(pngExport.Content);
        Assert.NotNull(bitmap);
    }

    private static ShapebankDefinitionStruct CreateFontDefinition(
        string name, (int width, int height)[] glyphSizes)
    {
        var graphics = glyphSizes.Select((size, i) =>
        {
            var pixelCount = size.width * size.height;
            var pixelData = new byte[pixelCount * 3];
            for (var p = 0; p < pixelCount; p++)
            {
                pixelData[p * 3] = (byte)((i * 50 + p * 30) % 256);
                pixelData[p * 3 + 1] = (byte)((i * 80 + p * 20) % 256);
                pixelData[p * 3 + 2] = (byte)((i * 110 + p * 10) % 256);
            }

            return new GraphicStruct
            {
                Width = (ushort)size.width,
                Height = (ushort)size.height,
                Width2 = (ushort)size.width,
                Height2 = (ushort)size.height,
                Size = (uint)(pixelData.Length + 48),
                SizeWithoutFooter = 0,
                PixelData = pixelData
            };
        }).ToArray();

        var shapebank = new ShapebankStruct
        {
            GraphicsCount = (ushort)graphics.Length,
            Graphics = graphics,
            UnknownData1 = []
        };

        return new ShapebankDefinitionStruct
        {
            Name = name,
            Shapebank = shapebank
        };
    }
}

public class FontIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public FontIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public async Task ConvertFonts_FromGameFiles_GeneratesAtlases()
    {
        var progress = new Progress<PipelineProgress>(p =>
            _output.WriteLine(p.ToString()));

        var outputPath = Path.Combine("output", "Fonts");

        await PipelineBuilder<GfxAsset>
            .Create()
            .DecodeWith<GfxDecoder>()
            .ConvertWith<GfxConverter>()
            .Write(new OutputHandlerOptions { OutputRoot = outputPath, OverwriteExisting = true })
            .Build()
            .ExecuteAsync(
                GameAssets.OfType<GfxAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath),
                progress);

        var fontDirs = Directory.GetDirectories(outputPath, "fonts", SearchOption.AllDirectories);
        Assert.NotEmpty(fontDirs);

        var fontDir = fontDirs[0];
        var pngFiles = Directory.GetFiles(fontDir, "*.png");
        var jsonFiles = Directory.GetFiles(fontDir, "*.json");
        Assert.NotEmpty(pngFiles);
        Assert.NotEmpty(jsonFiles);

        foreach (var jsonFile in jsonFiles)
        {
            var json = File.ReadAllText(jsonFile);
            var metadata = JsonConvert.DeserializeObject<FontMetadata>(json);
            Assert.NotNull(metadata);
            Assert.NotEmpty(metadata.Glyphs);
        }
    }
}
