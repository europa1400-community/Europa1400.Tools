namespace Europa1400.Tools.Pipeline.Assets;

public class SbfAsset : IGameAsset
{
    private readonly string? _relativePath;

    public string RelativePath
    {
        get => _relativePath ?? Path.GetFileName(FilePath);
        init => _relativePath = value;
    }

    public required string FilePath { get; init; }
}