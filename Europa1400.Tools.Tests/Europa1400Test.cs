using Europa1400.Tools.Pipeline;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Converter;
using Europa1400.Tools.Pipeline.Decoder;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Ageb;
using Europa1400.Tools.Structs.Aobj;
using Europa1400.Tools.Structs.Bgf;
using Europa1400.Tools.Structs.Gfx;
using Europa1400.Tools.Structs.Sbf;
using Xunit.Abstractions;

namespace Europa1400.Tools.Tests;

public class Europa1400Test(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestMeshes()
    {
        PipelineBuilder<BgfAsset>
            .Create()
            .DecodeWith<BgfDecoder, BgfStruct>()
            .Build()
            .Execute(GameAssets.OfType<BgfAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath));
    }

    [Fact]
    public void TestSounds()
    {
        var outputPath = Path.Combine("output", "sounds");

        PipelineBuilder<SbfAsset>
            .Create()
            .DecodeWith<SbfDecoder, SbfStruct>()
            .ConvertWith<SbfConverter, SbfStruct, List<IFileExport>>()
            .WriteTo(outputPath)
            .Build()
            .Execute(GameAssets.OfType<SbfAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath));

        Assert.True(Directory.GetFiles(outputPath, "*.wav", SearchOption.AllDirectories).Length > 0);
    }

    [Fact]
    public void TestGraphics()
    {
        var outputPath = Path.Combine("output", "graphics");

        PipelineBuilder<GfxAsset>
            .Create()
            .DecodeWith<GfxDecoder, GfxStruct>()
            .ConvertWith<GfxConverter, GfxStruct, List<IFileExport>>()
            .WriteTo(outputPath)
            .Build()
            .Execute(GameAssets.OfType<GfxAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath));

        Assert.True(Directory.GetFiles(outputPath, "*.png", SearchOption.AllDirectories).Length > 0);
    }

    [Fact]
    public void TestAgeb()
    {
        var outputPath = Path.Combine("output", "ageb");

        PipelineBuilder<AgebAsset>
            .Create()
            .DecodeWith<AgebDecoder, AgebStruct>()
            .WriteTo(outputPath)
            .Build()
            .Execute(GameAssets.OfType<AgebAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath));

        Assert.True(Directory.GetFiles(outputPath, "*.json", SearchOption.AllDirectories).Length > 0);
    }

    [Fact]
    public void TestAobj()
    {
        var outputPath = Path.Combine("output", "aobj");

        PipelineBuilder<AobjAsset>
            .Create()
            .DecodeWith<AobjDecoder, AobjStruct>()
            .WriteTo(outputPath)
            .Build()
            .Execute(GameAssets.OfType<AobjAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath));

        Assert.True(Directory.GetFiles(outputPath, "*.json", SearchOption.AllDirectories).Length > 0);
    }
}