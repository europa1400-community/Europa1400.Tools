using System;

namespace Europa1400.Tools.Pipeline.Assets
{
    internal class DummyAsset : GameAsset
    {
        public DummyAsset(string relativePath = "dummy.bin") : base(relativePath, relativePath)
        {
        }
    }
}