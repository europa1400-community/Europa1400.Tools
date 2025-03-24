using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Decoder
{
    public interface IDecoder<in TAsset, out TDecoded>
        where TAsset : IGameAsset
    {
        TDecoded Decode(TAsset asset);
    }
}