using System.Drawing;
using System.Drawing.Imaging;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Gfx;

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
                var bitmap = ConvertGraphic(graphic);

                using var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);

                var fileName = $"{baseName}_{i}.png";
                var filePath = Path.Combine(baseName, fileName).Replace('\\', '/');

                exports.Add(new FileExport { FilePath = filePath, Content = ms.ToArray() });
            }
        }

        return exports;
    }

    private Bitmap ConvertGraphic(GraphicStruct graphic)
    {
        var bmp = new Bitmap(graphic.Width, graphic.Height);

        if (graphic.PixelData != null)
        {
            int row = 0, col = 0;
            for (var i = 0; i < graphic.Width * graphic.Height * 3; i += 3)
            {
                var color = Color.FromArgb(255, graphic.PixelData[i], graphic.PixelData[i + 1],
                    graphic.PixelData[i + 2]);
                bmp.SetPixel(row, col, color);
                row = (row + 1) % graphic.Width;
                if (row == 0) col++;
            }
        }
        else if (graphic.GraphicRows != null)
        {
            var transparent = Color.FromArgb(0, 255, 255, 255);
            var pixels = new List<Color>();

            foreach (var row in graphic.GraphicRows)
            foreach (var block in row.TransparencyBlocks)
            {
                pixels.AddRange(Enumerable.Repeat(transparent, (int)(block.Size / 3)));
                for (var i = 0; i < block.Data.Length; i += 3)
                    pixels.Add(Color.FromArgb(255, block.Data[i], block.Data[i + 1], block.Data[i + 2]));
            }

            if (pixels.Count != graphic.Width * graphic.Height)
                throw new InvalidOperationException("Pixel count mismatch");

            int rowIdx = 0, colIdx = 0;
            foreach (var px in pixels)
            {
                bmp.SetPixel(rowIdx, colIdx, px);
                rowIdx = (rowIdx + 1) % graphic.Width;
                if (rowIdx == 0) colIdx++;
            }
        }

        return bmp;
    }
}