using Europa1400.Tools.Pipeline;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Converter;
using Europa1400.Tools.Pipeline.Decoder;
using Europa1400.Tools.Pipeline.Output;
using Xunit.Abstractions;

namespace Europa1400.Tools.Tests;

public class Europa1400Test(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task TestProgress()
    {
        var progressUpdates = new List<PipelineProgress>();

        var progress = new Progress<PipelineProgress>(p =>
        {
            progressUpdates.Add(p);
            testOutputHelper.WriteLine(p.ToString());
        });

        var pipeline = PipelineBuilder<DummyAsset>
            .Create()
            .DecodeWith<DummyDecoder>()
            .ConvertWith<DummyConverter>()
            .Write()
            .Build();

        await pipeline.ExecuteAsync(
            GameAssets.OfType<DummyAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath),
            progress
        );

        Assert.NotEmpty(progressUpdates);
    }

    [Fact(Skip = "This test does not work yet.")]
    public async Task TestMeshes()
    {
        await PipelineBuilder<BgfAsset>
            .Create()
            .DecodeWith<BgfDecoder>()
            .Build()
            .ExecuteAsync(GameAssets.OfType<BgfAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath));
    }

    [Fact]
    public async Task TestSounds()
    {
        var outputPath = Path.Combine("output", "Sounds");
        var progress = new Progress<PipelineProgress>(p => testOutputHelper.WriteLine(p.ToString()));

        await PipelineBuilder<SbfAsset>
            .Create()
            .DecodeWith<SbfDecoder>()
            .ConvertWith<SbfConverter>()
            .Write(new OutputHandlerOptions { OutputRoot = outputPath })
            .Build()
            .ExecuteAsync(GameAssets.OfType<SbfAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath), progress);

        Assert.True(Directory.GetFiles(outputPath, "*.wav", SearchOption.AllDirectories).Length > 0);
    }

    [Fact]
    public async Task TestGraphics()
    {
        var outputPath = Path.Combine("output", "Graphics");
        var progress = new Progress<PipelineProgress>(p => testOutputHelper.WriteLine(p.ToString()));

        await PipelineBuilder<GfxAsset>
            .Create()
            .DecodeWith<GfxDecoder>()
            .ConvertWith<GfxConverter>()
            .Write(new OutputHandlerOptions { OutputRoot = outputPath })
            .Build()
            .ExecuteAsync(GameAssets.OfType<GfxAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath), progress);

        Assert.True(Directory.GetFiles(outputPath, "*.png", SearchOption.AllDirectories).Length > 0);
    }

    [Fact]
    public async Task TestAgeb()
    {
        var outputPath = Path.Combine("output");
        var progress = new Progress<PipelineProgress>(p => testOutputHelper.WriteLine(p.ToString()));

        await PipelineBuilder<AgebAsset>
            .Create()
            .DecodeWith<AgebDecoder>()
            .Write(new OutputHandlerOptions { OutputRoot = outputPath })
            .Build()
            .ExecuteAsync(GameAssets.OfType<AgebAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath),
                progress);

        Assert.True(Directory.GetFiles(outputPath, "*.json", SearchOption.AllDirectories).Length > 0);
    }

    [Fact]
    public async Task TestAobj()
    {
        var outputPath = Path.Combine("output");
        var progress = new Progress<PipelineProgress>(p => testOutputHelper.WriteLine(p.ToString()));

        await PipelineBuilder<AobjAsset>
            .Create()
            .DecodeWith<AobjDecoder>()
            .Write(new OutputHandlerOptions { OutputRoot = outputPath })
            .Build()
            .ExecuteAsync(GameAssets.OfType<AobjAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath),
                progress);

        Assert.True(Directory.GetFiles(outputPath, "*.json", SearchOption.AllDirectories).Length > 0);
    }

    [Fact]
    public async Task TestAll()
    {
        var progress = new Progress<PipelineProgress>(p => testOutputHelper.WriteLine(p.ToString()));

        var outputPath = Path.Combine("output");
        await PipelineBuilder<AgebAsset>
            .Create()
            .DecodeWith<AgebDecoder>()
            .Write(new OutputHandlerOptions { OutputRoot = outputPath })
            .Build()
            .ExecuteAsync(GameAssets.OfType<AgebAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath),
                progress);

        outputPath = Path.Combine("output");
        await PipelineBuilder<AobjAsset>
            .Create()
            .DecodeWith<AobjDecoder>()
            .Write(new OutputHandlerOptions { OutputRoot = outputPath })
            .Build()
            .ExecuteAsync(GameAssets.OfType<AobjAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath),
                progress);

        outputPath = Path.Combine("output", "Sounds");
        await PipelineBuilder<SbfAsset>
            .Create()
            .DecodeWith<SbfDecoder>()
            .ConvertWith<SbfConverter>()
            .Write(new OutputHandlerOptions { OutputRoot = outputPath })
            .Build()
            .ExecuteAsync(GameAssets.OfType<SbfAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath), progress);

        outputPath = Path.Combine("output", "Graphics");
        await PipelineBuilder<GfxAsset>
            .Create()
            .DecodeWith<GfxDecoder>()
            .ConvertWith<GfxConverter>()
            .Write(new OutputHandlerOptions { OutputRoot = outputPath })
            .Build()
            .ExecuteAsync(GameAssets.OfType<GfxAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath), progress);
    }
}