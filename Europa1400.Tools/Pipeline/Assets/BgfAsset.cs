namespace Europa1400.Tools.Pipeline.Assets;

public class BgfAsset : IGameAsset
{
    private readonly string? _relativePath;

    public string RelativePath
    {
        get => _relativePath ?? Path.GetFileName(FilePath);
        init => _relativePath = value;
    }

    public TxsAsset? Txs { get; init; }
    public List<TextureAsset>? Textures { get; init; }
    public List<BafAsset>? Animations { get; init; }

    public required string FilePath { get; init; }
}