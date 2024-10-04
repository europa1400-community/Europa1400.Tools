namespace Europa1400.Tools;

using System.IO;

public class Europa1400(DirectoryInfo gameDirectory)
{
    public DirectoryInfo GameDirectory { get; init; } = gameDirectory;

    public static Europa1400 FromDirectory(DirectoryInfo gameDirectory)
    {
        if (!gameDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"The directory {gameDirectory.FullName} does not exist.");
        }

        var europa1400 = new Europa1400(gameDirectory);

        return europa1400;
    }

    public static Europa1400 FromPath(string path)
    {
        var directoryInfo = new DirectoryInfo(path);
        return FromDirectory(directoryInfo);
    }
}
