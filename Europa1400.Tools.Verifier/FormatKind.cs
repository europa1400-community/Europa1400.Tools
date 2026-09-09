using Europa1400.Tools.Pipeline;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Output;

namespace Europa1400.Tools.Verifier;

internal sealed class ConverterChoice
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }

    public required Func<GameAsset, object, IProgress<PipelineProgress>?, CancellationToken,
        Task<IReadOnlyList<IFileExport>>> Convert { get; init; }
}

internal sealed class FormatKind
{
    public required string Id { get; init; }
    public required string Category { get; init; }
    public required Func<string, IReadOnlyList<GameAsset>> Discover { get; init; }
    public required Func<GameAsset, CancellationToken, Task<object>> Decode { get; init; }
    public IReadOnlyList<ConverterChoice> Converters { get; init; } = [];
}
