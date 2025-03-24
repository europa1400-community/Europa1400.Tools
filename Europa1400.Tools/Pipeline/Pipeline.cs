using System;
using System.Collections.Generic;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline
{
    public class Pipeline<TAsset> where TAsset : IGameAsset
    {
        private readonly Func<TAsset, object>? decode;
        private Func<object, object>? convert;
        private Action<object, TAsset>? write;

        public Pipeline(Func<TAsset, object>? decode, Func<object, object>? convert, Action<object, TAsset>? write)
        {
            this.convert = convert;
            this.decode = decode;
            this.write = write;
        }
        
        public void Execute(AssetSelection<TAsset> selection)
        {
            foreach (var asset in selection.Assets)
            {
                object current = asset;

                if (decode != null)
                    current = decode(asset);

                if (convert != null)
                    current = convert(current);

                write?.Invoke(current, asset);
            }
        }

        public void Execute(TAsset asset)
        {
            Execute(new AssetSelection<TAsset>(new List<TAsset> { asset }.AsReadOnly()));
        }

        public void Execute(IEnumerable<TAsset> assets)
        {
            Execute(new AssetSelection<TAsset>(assets));
        }
    }
}