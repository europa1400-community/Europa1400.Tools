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

            foreach (var shapebank in gfxStruct.ShapebankDefinitions)
            {
                var convertedShapebank = ConvertShapebank(shapebank);

                shapebankImages.Add(shapebank.Name, convertedShapebank);
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

    private Dictionary<string, Image> ConvertShapebank(ShapebankDefinitionStruct shapebank)
    {
        var result = new Dictionary<string, Image>();

        if(shapebank.Shapebank == null)
            throw new InvalidOperationException("Shapebank is null");

        var baseName = shapebank.Name;

        for (var i = 0; i < shapebank.Shapebank.Graphics.Count; i++)
        {
            var graphic = shapebank.Shapebank.Graphics[i];
            var convertedGraphic = ConvertGraphic(graphic);

            if(convertedGraphic != null)
                result.Add($"{baseName}_{i}", convertedGraphic);
        }

        return result;
    }

    private Image? ConvertGraphic(GraphicStruct graphic)
    {
        Image? result = null;

        if (graphic.PixelData != null)
        {
            // TODO
        }
        else if(graphic.GraphicRows != null)
        {
            // TODO
        }
        else
            throw new ArgumentException("Graphic has no pixel data or graphic rows");

        return result;
    }
}
