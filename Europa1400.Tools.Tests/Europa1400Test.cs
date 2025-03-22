using Europa1400.Tools.Converter;

namespace Europa1400.Tools.Tests;

public class Europa1400Test
{
    [Fact]
    public void TestSoundConverter()
    {
        var path = EnvVariables.GameDirectoryPath;

        var soundConverter = new SoundConverter();
        var tempTargetPath = Directory.CreateTempSubdirectory("Sound");

        soundConverter.Convert(path, tempTargetPath.FullName);

        var tempTargetPathSound = new DirectoryInfo(Path.Combine(tempTargetPath.FullName, "Sound"));

        Assert.True(tempTargetPathSound.Exists);
        Assert.True(tempTargetPathSound.GetFiles("*.wav", SearchOption.AllDirectories).Length > 0);

        tempTargetPath.Delete(true);
    }

    [Fact]
    public void TestGraphicsConverter()
    {
        var path = EnvVariables.GameDirectoryPath;

        var graphicsConverter = new GraphicsConverter();
        var tempTargetPath = Directory.CreateTempSubdirectory("Graphics");

        graphicsConverter.Convert(path, tempTargetPath.FullName);

        var tempTargetPathGraphics = new DirectoryInfo(Path.Combine(tempTargetPath.FullName, "Graphics"));

        Assert.True(tempTargetPathGraphics.Exists);
        //Assert.True(tempTargetPathGraphics.GetFiles("*.png", SearchOption.AllDirectories).Length > 0);

        tempTargetPath.Delete(true);
    }

    [Fact]
    public void TestAgebConverter()
    {
        var path = EnvVariables.GameDirectoryPath;
        var agebConverter = new AgebConverter();
        var tempTargetPath = Directory.CreateTempSubdirectory("Ageb");

        agebConverter.Convert(path, tempTargetPath.FullName);

        Assert.True(tempTargetPath.Exists);
        Assert.True(tempTargetPath.GetFiles("*.json", SearchOption.AllDirectories).Length > 0);
    }

    [Fact]
    public void TestAobjConverter()
    {
        var path = EnvVariables.GameDirectoryPath;
        var aobjConverter = new AobjConverter();
        var tempTargetPath = Directory.CreateTempSubdirectory("Aobj");

        aobjConverter.Convert(path, tempTargetPath.FullName);

        Assert.True(tempTargetPath.Exists);
        Assert.True(tempTargetPath.GetFiles("*.json", SearchOption.AllDirectories).Length > 0);
    }

    [Fact]
    public void TestBgfConverter()
    {
        var path = EnvVariables.GameDirectoryPath;
        var bgfConverter = new BgfConverter
        {
            PathToGameFiles = path,
            TargetDirectory = Directory.CreateTempSubdirectory("Bgf").FullName
        };

        bgfConverter.Convert();
    }
}