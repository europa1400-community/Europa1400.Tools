using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Gfx;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Europa1400.Tools.Pipeline.Converter;

public class GfxConverter : IConverter<GfxStruct, List<IFileExport>>
{
    public List<IFileExport> Convert(GfxStruct input)
    {
        var exports = new List<IFileExport>();

        foreach (var shapebankDef in input.ShapebankDefinitions)
        {
            var shapebank = shapebankDef.Shapebank;
            if (shapebank == null)
                continue;

            var baseName = shapebankDef.Name;

            for (var i = 0; i < shapebank.Graphics.Length; i++)
            {
                var graphic = shapebank.Graphics[i];
                var image = ConvertGraphic(graphic);

                using var ms = new MemoryStream();
                image.SaveAsPng(ms);

                var fileName = $"{baseName}_{i}.png";
                var filePath = Path.Combine(baseName, fileName).Replace('\\', '/');

                exports.Add(new FileExport
                {
                    FilePath = filePath,
                    Content = ms.ToArray()
                });
            }
        }

        return exports;
    }

    private Image<Rgba32> ConvertGraphic(GraphicStruct graphic)
    {
        var image = new Image<Rgba32>(graphic.Width, graphic.Height);

        if (graphic.PixelData != null)
        {
            int row = 0, col = 0;

            for (var i = 0; i < graphic.PixelData.Length; i += 3)
            {
                var color = new Rgba32(graphic.PixelData[i], graphic.PixelData[i + 1], graphic.PixelData[i + 2]);
                image[row, col] = color;

                row = (row + 1) % graphic.Width;
                if (row == 0) col++;
            }
        }
        else if (graphic.GraphicRows != null)
        {
            var transparent = new Rgba32(255, 255, 255, 0);
            var pixels = new List<Rgba32>();

            foreach (var graphicRow in graphic.GraphicRows)
            foreach (var block in graphicRow.TransparencyBlocks)
            {
                for (var i = 0; i < block.Size; i += 3)
                    pixels.Add(transparent);

                for (var i = 0; i < block.Data.Length; i += 3)
                {
                    var color = new Rgba32(block.Data[i], block.Data[i + 1], block.Data[i + 2], 255);
                    pixels.Add(color);
                }
            }

            if (pixels.Count != graphic.Width * graphic.Height)
                throw new InvalidOperationException("Pixel count mismatch");

            int row = 0, col = 0;
            foreach (var px in pixels)
            {
                image[row, col] = px;
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