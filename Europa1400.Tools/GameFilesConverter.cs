using Europa1400.Tools.Converter;
using Europa1400.Tools.Interfaces;

namespace Europa1400.Tools;

public static class GameFilesConverter
{
    private static readonly List<IConverter> Converters = [
        new SoundConverter(),
        new GraphicsConverter()
    ];

    public static void Convert(string pathToGameFiles, string targetPath)
    {
        var sourceDirectoy = new DirectoryInfo(pathToGameFiles);
        var targetDirectoy = new DirectoryInfo(targetPath);

        if (!sourceDirectoy.Exists)
            throw new DirectoryNotFoundException(String.Format(Resources.Error_GameFileDirectoryNotFound, pathToGameFiles));

        if (!targetDirectoy.Exists)
        {
            targetDirectoy.Create();
        }
        else if (targetDirectoy.GetFiles().Length > 0)
        {
            targetDirectoy.Delete(true);
            targetDirectoy.Create();
        }

        Converters.ForEach(m => m.Convert(pathToGameFiles, targetPath));
    }
}
