using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Output;
using Newtonsoft.Json;

namespace Europa1400.Tools.Verifier;

internal sealed class CachedPreview
{
    public required string Json { get; init; }
    public required IReadOnlyList<PreviewFile> Files { get; init; }
}

internal sealed class PreviewFile
{
    public required string RelativePath { get; init; }
    public required string CachePath { get; init; }
    public required PreviewKind Kind { get; init; }
    public required long ByteLength { get; init; }
}

internal enum PreviewKind
{
    Image,
    Audio,
    Text,
    Model,
    Binary
}

internal sealed class VerifierCache
{
    private readonly string _libraryFingerprint;
    private readonly string _root;

    public VerifierCache(string root, string libraryFingerprint)
    {
        _root = root;
        _libraryFingerprint = libraryFingerprint;
        Directory.CreateDirectory(_root);
    }

    public void Invalidate(FormatKind format, GameAsset asset, ConverterChoice? converter)
    {
        var directory = AssetDirectory(format, asset, converter);
        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
    }

    public bool TryGet(FormatKind format, GameAsset asset, ConverterChoice? converter, out CachedPreview? preview)
    {
        preview = null;
        var directory = AssetDirectory(format, asset, converter);
        var stampPath = Path.Combine(directory, "_stamp.txt");
        var jsonPath = Path.Combine(directory, "_decoded.json");
        var manifestPath = Path.Combine(directory, "_files.json");

        if (!File.Exists(stampPath) || File.ReadAllText(stampPath) != Stamp(asset))
            return false;

        if (!File.Exists(jsonPath) || !File.Exists(manifestPath))
            return false;

        var relativePaths = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(manifestPath)) ?? [];
        var files = new List<PreviewFile>();

        foreach (var relativePath in relativePaths)
        {
            var cachePath = Path.Combine(directory, "exports", relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(cachePath))
                return false;

            files.Add(CreatePreviewFile(relativePath, cachePath));
        }

        preview = new CachedPreview
        {
            Json = File.ReadAllText(jsonPath),
            Files = files
        };
        return true;
    }

    public CachedPreview Store(FormatKind format, GameAsset asset, ConverterChoice? converter, string json, IEnumerable<IFileExport> exports)
    {
        var directory = AssetDirectory(format, asset, converter);
        var exportRoot = Path.Combine(directory, "exports");

        if (Directory.Exists(directory))
            Directory.Delete(directory, true);

        Directory.CreateDirectory(exportRoot);

        var files = new List<PreviewFile>();
        var relativePaths = new List<string>();

        foreach (var export in exports)
        {
            var relativePath = export.FilePath.Replace('\\', '/');
            var cachePath = Path.Combine(exportRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
            File.WriteAllBytes(cachePath, export.Content);
            relativePaths.Add(relativePath);
            files.Add(CreatePreviewFile(relativePath, cachePath));
        }

        File.WriteAllText(Path.Combine(directory, "_decoded.json"), json);
        File.WriteAllText(Path.Combine(directory, "_files.json"), JsonConvert.SerializeObject(relativePaths));
        File.WriteAllText(Path.Combine(directory, "_stamp.txt"), Stamp(asset));

        return new CachedPreview { Json = json, Files = files };
    }

    private string AssetDirectory(FormatKind format, GameAsset asset, ConverterChoice? converter)
    {
        var relative = string.IsNullOrWhiteSpace(asset.RelativePath) ? Path.GetFileName(asset.FilePath) : asset.RelativePath;
        var safe = relative.Replace(':', '_').Replace('\\', Path.DirectorySeparatorChar);
        var converterId = converter?.Id ?? "_decode";
        return Path.Combine(_root, format.Id, converterId, safe);
    }

    private string Stamp(GameAsset asset)
    {
        var info = new FileInfo(asset.FilePath);
        var source = info.Exists ? $"{info.LastWriteTimeUtc.Ticks}:{info.Length}" : "missing";
        return $"{_libraryFingerprint}|{source}";
    }

    private static PreviewFile CreatePreviewFile(string relativePath, string cachePath)
    {
        var ext = Path.GetExtension(cachePath).ToLowerInvariant();
        var kind = ext switch
        {
            ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" => PreviewKind.Image,
            ".wav" or ".mp3" => PreviewKind.Audio,
            ".glb" or ".gltf" or ".obj" => PreviewKind.Model,
            ".json" or ".txt" or ".xml" or ".mtl" => PreviewKind.Text,
            _ => PreviewKind.Binary
        };

        return new PreviewFile
        {
            RelativePath = relativePath,
            CachePath = cachePath,
            Kind = kind,
            ByteLength = new FileInfo(cachePath).Length
        };
    }
}
