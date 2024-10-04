namespace Europa1400.Tools.Tests;

public class Europa1400Test
{
    [Fact]
    public void TestFromPath()
    {
        var path = EnvVariables.GameDirectoryPath;
        var europa1400 = Europa1400.FromPath(path);

        Assert.Equal(path, europa1400.GameDirectory.FullName);
    }

    [Fact]
    public void TestFromDirectory()
    {
        var path = EnvVariables.GameDirectoryPath;
        var directoryInfo = new DirectoryInfo(path);
        var europa1400 = Europa1400.FromDirectory(directoryInfo);

        Assert.Equal(path, europa1400.GameDirectory.FullName);
    }
}
