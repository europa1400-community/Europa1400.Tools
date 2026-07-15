using System.Collections.Generic;

namespace Europa1400.Tools.Pipeline.Assets
{
    public class BgfAsset : GameAsset
    {
        public BgfAsset(string filePath, string relativePath, TxsAsset? txs, List<TextureAsset>? textures,
            List<BafAsset>? animations) : base(filePath, relativePath)
        {
            Txs = txs;
            Textures = textures;
            Animations = animations;
        }

        public TxsAsset? Txs { get; set; }
        public List<TextureAsset>? Textures { get; set; }
        public List<BafAsset>? Animations { get; set; }
    }
}