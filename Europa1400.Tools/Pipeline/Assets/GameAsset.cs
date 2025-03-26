namespace Europa1400.Tools.Pipeline.Assets
{
    public abstract class GameAsset
    {
        protected GameAsset(string filePath, string relativePath)
        {
            FilePath = filePath;
            RelativePath = relativePath;
        }

        public string FilePath { get; set; }
        public string RelativePath { get; set; }
    }
}