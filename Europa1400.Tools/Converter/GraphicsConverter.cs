using Europa1400.Tools.Decoder.Gfx;
using Europa1400.Tools.Interfaces;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Europa1400.Tools.Tests")]

namespace Europa1400.Tools.Converter;

#pragma warning disable CA1416 // Validate platform compatibility
internal class GraphicsConverter : IConverter
{
    public void Convert(string pathToGameFiles, string targetDirectory)
    {
        var sourceDirectory = new DirectoryInfo(pathToGameFiles);

        var outputPath = Path.Combine(targetDirectory, "Graphics");
        var graphicsDirectory = new DirectoryInfo(outputPath);
        graphicsDirectory.Create();

        var graphicsFiles = sourceDirectory.GetFiles("*.gfx", SearchOption.AllDirectories);

        foreach (var graphicsFile in graphicsFiles)
        {
            using var br = new BinaryReader(File.OpenRead(graphicsFile.FullName));
            var gfxStruct = GfxStruct.FromBytes(br);

            Dictionary<string, Dictionary<string, Image>> shapebankImages = [];

            foreach (var shapebankDefinition in gfxStruct.ShapebankDefinitions)
            {
                if (shapebankDefinition.Shapebank == null)
                    continue;

                var convertedShapebank = ConvertShapebank(shapebankDefinition.Name, shapebankDefinition.Shapebank);

                shapebankImages.Add(shapebankDefinition.Name, convertedShapebank);
            }

            foreach (var shapebank in shapebankImages)
            {
                var shapebankDirectory = Path.Combine(outputPath, shapebank.Key);
                var shapebankDirectoryInfo = new DirectoryInfo(shapebankDirectory);
                shapebankDirectoryInfo.Create();

                foreach (var image in shapebank.Value)
                {
                    image.Value.Save(Path.Combine(shapebankDirectory, $"{image.Key}.png"), ImageFormat.Png);
                }
            }
        }
    }

    private Dictionary<string, Image> ConvertShapebank(string baseName, ShapebankStruct shapebank)
    {
        var result = new Dictionary<string, Image>();

        for (var i = 0; i < shapebank.Graphics.Length; i++)
        {
            var graphic = shapebank.Graphics[i];
            var convertedGraphic = ConvertGraphic(graphic);

            result.Add($"{baseName}_{i}", convertedGraphic);
        }

        return result;
    }

    private Image ConvertGraphic(GraphicStruct graphic)
    {
        var result = new Bitmap(graphic.Width, graphic.Height);

        if (graphic.PixelData != null)
        {
            var row = 0;
            var col = 0;

            for (var i = 0; i < graphic.Width * graphic.Height; i += 3)
            {
                var pixel = Color.FromArgb(255, graphic.PixelData[i], graphic.PixelData[i + 1],
                    graphic.PixelData[i + 2]);

                result.SetPixel(row, col, pixel);

                if (row == graphic.Width - 1)
                {
                    row = 0;
                    col++;
                }
                else
                    row++;
            }
        }
        else if (graphic.GraphicRows != null)
        {
            var transparent = Color.FromArgb(0, 255, 255, 255);
            var imageData = new List<Color>();

            foreach (var graphicRow in graphic.GraphicRows)
            {
                foreach (var transparencyBlock in graphicRow.TransparencyBlocks)
                {
                    for (var k = 0; k < transparencyBlock.Size; k += 3)
                    {
                        imageData.Add(transparent);
                    }

                    for (var k = 0; k < transparencyBlock.Data.Length; k += 3)
                    {
                        imageData.Add(Color.FromArgb(255, transparencyBlock.Data[k], transparencyBlock.Data[k + 1],
                            transparencyBlock.Data[k + 2]));
                    }
                }
            }

            if (imageData.Count != graphic.Height * graphic.Width)
                throw new Exception(
                    $"Image data does not match graphic dimensions. Expected {graphic.Height * graphic.Width}, got {imageData.Count}");

            var row = 0;
            var col = 0;

            foreach (var pixel in imageData)
            {
                result.SetPixel(row, col, pixel);

                if (row == graphic.Width - 1)
                {
                    row = 0;
                    col++;
                }
                else
                    row++;
            }
        }
        else
            throw new ArgumentException("Graphic has no pixel data or graphic rows");

        return result;
    }
}