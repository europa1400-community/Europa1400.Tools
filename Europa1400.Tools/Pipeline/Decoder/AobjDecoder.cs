using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Structs.Aobj;

namespace Europa1400.Tools.Pipeline.Decoder;

public class AobjDecoder : IDecoder<AobjAsset, AobjStruct>
{
    public AobjStruct Decode(AobjAsset asset)
    {
        using var stream = File.OpenRead(asset.FilePath);
        using var reader = new BinaryReader(stream);
        return AobjStruct.FromBytes(reader);
    }
}