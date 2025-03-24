using System.Collections.Generic;
using System.IO;

namespace Europa1400.Tools.Pipeline.Assets
{
    public class BgfAsset : IGameAsset
    {
        private string? _relativePath;

        public string RelativePath
        {
            get => _relativePath ?? Path.GetFileName(FilePath);
            set => _relativePath = value;
        }

        public TxsAsset? Txs { get; set; }
        public List<TextureAsset>? Textures { get; set; }
        public List<BafAsset>? Animations { get; set; }

        public string FilePath { get; set; }
    }
}