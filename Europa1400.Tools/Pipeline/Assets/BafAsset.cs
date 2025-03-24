namespace Europa1400.Tools.Pipeline.Assets;

public class BafAsset : IGameAsset
{
    public BafIniAsset? Descriptor { get; set; }
    public required string FilePath { get; init; }
}