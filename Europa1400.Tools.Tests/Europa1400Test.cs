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
}
