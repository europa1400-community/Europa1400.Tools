using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline;

public class Pipeline<TAsset>(
    Func<TAsset, object>? decode,
    Func<object, object>? convert,
    Action<object, TAsset>? write)
    where TAsset : IGameAsset
{
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
        Execute(new AssetSelection<TAsset>([asset]));
    }

    public void Execute(IEnumerable<TAsset> assets)
    {
        Execute(new AssetSelection<TAsset>(assets));
    }
}