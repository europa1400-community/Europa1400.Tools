using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Gfx;
using SkiaSharp;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class GfxConverter : IConverter<GfxStruct, List<IFileExport>>
    {
        public Task<List<IFileExport>> ConvertAsync(GfxStruct input, CancellationToken cancellationToken = default)
        {
            var exports = new List<IFileExport>();

            foreach (var shapebankDef in input.ShapebankDefinitions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var shapebank = shapebankDef.Shapebank;
                if (shapebank == null)
                    continue;

                var baseName = shapebankDef.Name;

                for (var i = 0; i < shapebank.Graphics.Length; i++)
                {
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
                }
            }

            return Task.FromResult(exports);
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