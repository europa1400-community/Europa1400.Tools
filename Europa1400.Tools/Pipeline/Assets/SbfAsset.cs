using System.IO;

namespace Europa1400.Tools.Pipeline.Assets
{
    public class SbfAsset : IGameAsset
    {
        private string? _relativePath;

        public string RelativePath
        {
            get => _relativePath ?? Path.GetFileName(FilePath);
            set => _relativePath = value;
        }

        public string FilePath { get; set; }
    }
}