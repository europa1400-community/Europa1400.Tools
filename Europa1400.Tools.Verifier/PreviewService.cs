using Europa1400.Tools.Pipeline;
using Europa1400.Tools.Pipeline.Assets;
using Newtonsoft.Json;

namespace Europa1400.Tools.Verifier;

internal sealed class PreviewService
{
    private readonly VerifierCache _cache;
    private readonly JsonSerializer _jsonSerializer;

    public PreviewService(VerifierCache cache)
    {
        _cache = cache;
        _jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = { new ByteArrayPreviewConverter() }
        });
    }

    public async Task<CachedPreview> LoadAsync(
        FormatKind format,
        ConverterChoice? converter,
        GameAsset asset,
        IProgress<PipelineProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (_cache.TryGet(format, asset, converter, out var cached) && cached != null)
            return cached;

        progress?.Report(new PipelineProgress
        {
            Step = PipelineStep.Decode,
            FileName = asset.RelativePath
        });

        var decoded = await format.Decode(asset, cancellationToken).ConfigureAwait(false);
        var json = ToJson(decoded);
        IReadOnlyList<Pipeline.Output.IFileExport> exports = [];

        if (converter != null)
            exports = await converter.Convert(asset, decoded, progress, cancellationToken).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        return _cache.Store(format, asset, converter, json, exports);
    }

    private string ToJson(object decoded)
    {
        using var writer = new StringWriter();
        _jsonSerializer.Serialize(writer, decoded);
        return writer.ToString();
    }

    private sealed class ByteArrayPreviewConverter : JsonConverter<byte[]>
    {
        public override void WriteJson(JsonWriter writer, byte[]? value, JsonSerializer serializer)
        {
            writer.WriteValue(value is null ? null : $"<{value.Length} bytes>");
        }

        public override byte[] ReadJson(JsonReader reader, Type objectType, byte[]? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
