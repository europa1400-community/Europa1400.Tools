namespace Europa1400.Tools.Pipeline.Assets
{
    public class BafAsset : GameAsset
    {
        public BafAsset(string filePath, string relativePath, BafIniAsset? bafIni) : base(filePath, relativePath)
        {
            BafIni = bafIni;
        }

        public BafIniAsset? BafIni { get; set; }
    }
}