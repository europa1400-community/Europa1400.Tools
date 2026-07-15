using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Font;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Gfx;
using Newtonsoft.Json;
using SkiaSharp;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class GfxConverter : IConverter
    {
        public int EstimateSteps(object input)
        {
            if (!(input is ShapebankDefinitionStruct shapebankDefinition))
                throw new ArgumentException("Input is not of type ShapebankDefinitionStruct");

            return shapebankDefinition.Shapebank?.Graphics.Length ?? 0;
        }

        public Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default)
        {
            if (!(input is ShapebankDefinitionStruct shapebankDefinition))
                throw new ArgumentException("Input is not of type ShapebankDefinitionStruct");

            cancellationToken.ThrowIfCancellationRequested();

            if (shapebankDefinition.IsFont)
                return ConvertFontAsync(shapebankDefinition, pipelineProgress, progress, cancellationToken);

            var exports = new List<IFileExport>();

            var shapebank = shapebankDefinition.Shapebank;
            if (shapebank == null) return Task.FromResult(exports.AsEnumerable());

            var baseName = shapebankDefinition.Name;

            for (var i = 0; i < shapebank.Graphics.Length; i++)
            {
                pipelineProgress.FileName = Path.Combine(baseName, $"{baseName}_{i}.png");
                progress?.Report(pipelineProgress);
                cancellationToken.ThrowIfCancellationRequested();

                var graphic = shapebank.Graphics[i];
                var image = ConvertGraphic(graphic);

                using var ms = new MemoryStream();
                image.Encode(ms, SKEncodedImageFormat.Png, 100);
                ms.Seek(0, SeekOrigin.Begin);

                var fileName = $"{baseName}_{i}.png";
                var filePath = Path.Combine(baseName, fileName).Replace('\\', '/');

                exports.Add(new FileExport
                {
                    FilePath = filePath,
                    Content = ms.ToArray()
                });

                pipelineProgress.Current += 1;
                progress?.Report(pipelineProgress);
            }

            return Task.FromResult(exports.AsEnumerable());
        }

        public Task<IEnumerable<IFileExport>> ConvertFontAsync(
            ShapebankDefinitionStruct input,
            PipelineProgress? pipelineProgress = null,
            IProgress<PipelineProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var exports = new List<IFileExport>();
            var shapebank = input.Shapebank;

            if (shapebank?.Graphics == null || shapebank.Graphics.Length == 0)
                return Task.FromResult(exports.AsEnumerable());

            var baseName = input.Name;

            var glyphBitmaps = new List<(SKBitmap bitmap, int index)>();
            for (var i = 0; i < shapebank.Graphics.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (pipelineProgress != null)
                {
                    pipelineProgress.FileName = $"{baseName}_glyph_{i}";
                    progress?.Report(pipelineProgress);
                }

                var bitmap = ConvertGraphic(shapebank.Graphics[i]);
                glyphBitmaps.Add((bitmap, i));

                if (pipelineProgress != null)
                {
                    pipelineProgress.Current += 1;
                    progress?.Report(pipelineProgress);
                }
            }

            var widths = glyphBitmaps.Select(g => g.bitmap.Width).ToArray();
            var heights = glyphBitmaps.Select(g => g.bitmap.Height).ToArray();
            var (packed, atlasWidth, atlasHeight) = GlyphPacker.Pack(widths, heights);

            using var atlas = new SKBitmap(atlasWidth, atlasHeight);
            using var canvas = new SKCanvas(atlas);
            canvas.Clear(new SKColor(0, 0, 0, 0));

            foreach (var p in packed)
            {
                var (bitmap, _) = glyphBitmaps[p.Index];
                canvas.DrawBitmap(bitmap, p.X, p.Y);
            }

            var maxHeight = glyphBitmaps.Max(g => g.bitmap.Height);
            var lineHeight = maxHeight + 2;
            var baseLine = maxHeight;

            var fontMetadata = new FontMetadata
            {
                Name = baseName,
                AtlasWidth = atlasWidth,
                AtlasHeight = atlasHeight,
                LineHeight = lineHeight,
                BaseLine = baseLine,
                Glyphs = new List<FontGlyphInfo>()
            };

            foreach (var p in packed.OrderBy(p => p.Index))
            {
                fontMetadata.Glyphs.Add(new FontGlyphInfo
                {
                    CharCode = 32 + p.Index,
                    X = p.X,
                    Y = p.Y,
                    Width = p.Width,
                    Height = p.Height,
                    Advance = p.Width + 1,
                    OffsetX = 0,
                    OffsetY = baseLine - p.Height
                });
            }

            using (var ms = new MemoryStream())
            {
                atlas.Encode(ms, SKEncodedImageFormat.Png, 100);
                ms.Seek(0, SeekOrigin.Begin);
                exports.Add(new FileExport
                {
                    FilePath = $"fonts/{baseName}/{baseName}.png",
                    Content = ms.ToArray()
                });
            }

            var json = JsonConvert.SerializeObject(fontMetadata, Formatting.Indented);
            exports.Add(new FileExport
            {
                FilePath = $"fonts/{baseName}/{baseName}.json",
                Content = Encoding.UTF8.GetBytes(json)
            });

            foreach (var (bitmap, _) in glyphBitmaps)
                bitmap.Dispose();

            return Task.FromResult<IEnumerable<IFileExport>>(exports);
        }

        private SKBitmap ConvertGraphic(GraphicStruct graphic)
        {
            var image = new SKBitmap(graphic.Width, graphic.Height);

            if (graphic.PixelData != null)
            {
                int row = 0, col = 0;

                for (var i = 0; i < graphic.PixelData.Length; i += 3)
                {
                    var color = new SKColor(graphic.PixelData[i], graphic.PixelData[i + 1], graphic.PixelData[i + 2]);
                    image.SetPixel(row, col, color);

                    row = (row + 1) % graphic.Width;
                    if (row == 0) col++;
                }
            }
            else if (graphic.GraphicRows != null)
            {
                var transparent = new SKColor(255, 255, 255, 0);
                var pixels = new List<SKColor>();

                foreach (var graphicRow in graphic.GraphicRows)
                foreach (var block in graphicRow.TransparencyBlocks)
                {
                    for (var i = 0; i < block.Size; i += 3)
                        pixels.Add(transparent);

                    for (var i = 0; i < block.Data.Length; i += 3)
                    {
                        var color = new SKColor(block.Data[i], block.Data[i + 1], block.Data[i + 2], 255);
                        pixels.Add(color);
                    }
                }

                if (pixels.Count != graphic.Width * graphic.Height)
                    throw new InvalidOperationException("Pixel count mismatch");

                int row = 0, col = 0;
                foreach (var px in pixels)
                {
                    image.SetPixel(row, col, px);
                    row = (row + 1) % graphic.Width;
                    if (row == 0) col++;
                }
            }
            else
            {
                throw new InvalidOperationException("Graphic has no pixel data or graphic rows");
            }

            return image;
        }
    }
}
