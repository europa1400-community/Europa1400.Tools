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
    public async Task TestProgress()
    {
        var progressUpdates = new List<PipelineProgress>();

        var progress = new Progress<PipelineProgress>(p =>
        {
            progressUpdates.Add(p);
            testOutputHelper.WriteLine($"[{p.CurrentStep}/{p.TotalSteps}] {p.Percent:P0} - {p.Message}");
        });

        var pipeline = PipelineBuilder<DummyAsset>
            .Create()
            .DecodeWith<DummyDecoder, object>()
            .ConvertWith<DummyConverter, object, List<IFileExport>>()
            .WriteWith<DummyOutputHandler, List<IFileExport>>()
            .Build();

        await pipeline.ExecuteAsync(
            GameAssets.OfType<DummyAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath),
            progress
        );

        Assert.NotEmpty(progressUpdates);

        var final = progressUpdates[^1];
        Assert.Equal(final.TotalSteps, final.CurrentStep);
        Assert.Equal(1.0, final.Percent, 2);
    }

    [Fact(Skip = "This test does not work yet.")]
    public async Task TestMeshes()
    {
        await PipelineBuilder<BgfAsset>
            .Create()
            .DecodeWith<BgfDecoder, BgfStruct>()
            .Build()
            .ExecuteAsync(GameAssets.OfType<BgfAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath));
    }

    [Fact]
    public async Task TestSounds()
    {
        var outputPath = Path.Combine("output", "sounds");

        await PipelineBuilder<SbfAsset>
            .Create()
            .DecodeWith<SbfDecoder, SbfStruct>()
            .ConvertWith<SbfConverter, SbfStruct, List<IFileExport>>()
            .WriteWith<FileExportOutputHandler, List<IFileExport>>(new OutputHandlerOptions { OutputRoot = outputPath })
            .Build()
            .ExecuteAsync(GameAssets.OfType<SbfAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath));

        Assert.True(Directory.GetFiles(outputPath, "*.wav", SearchOption.AllDirectories).Length > 0);
    }

    [Fact]
    public async Task TestGraphics()
    {
        var outputPath = Path.Combine("output", "graphics");

        await PipelineBuilder<GfxAsset>
            .Create()
            .DecodeWith<GfxDecoder, GfxStruct>()
            .ConvertWith<GfxConverter, GfxStruct, List<IFileExport>>()
            .WriteWith<FileExportOutputHandler, List<IFileExport>>(new OutputHandlerOptions { OutputRoot = outputPath })
            .Build()
            .ExecuteAsync(GameAssets.OfType<GfxAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath));

        Assert.True(Directory.GetFiles(outputPath, "*.png", SearchOption.AllDirectories).Length > 0);
    }

    [Fact]
    public async Task TestAgeb()
    {
        var outputPath = Path.Combine("output", "ageb");

        await PipelineBuilder<AgebAsset>
            .Create()
            .DecodeWith<AgebDecoder, AgebStruct>()
            .WriteWith<FileExportOutputHandler, List<IFileExport>>(new OutputHandlerOptions { OutputRoot = outputPath })
            .Build()
            .ExecuteAsync(GameAssets.OfType<AgebAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath));

        Assert.True(Directory.GetFiles(outputPath, "*.json", SearchOption.AllDirectories).Length > 0);
    }

    [Fact]
    public async Task TestAobj()
    {
        var outputPath = Path.Combine("output", "aobj");

        await PipelineBuilder<AobjAsset>
            .Create()
            .DecodeWith<AobjDecoder, AobjStruct>()
            .WriteWith<FileExportOutputHandler, List<IFileExport>>(new OutputHandlerOptions { OutputRoot = outputPath })
            .Build()
            .ExecuteAsync(GameAssets.OfType<AobjAsset>().FromGameInstallation(EnvVariables.GameDirectoryPath));

        Assert.True(Directory.GetFiles(outputPath, "*.json", SearchOption.AllDirectories).Length > 0);
    }
}