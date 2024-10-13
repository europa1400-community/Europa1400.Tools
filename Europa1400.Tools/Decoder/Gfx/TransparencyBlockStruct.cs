using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Gfx;

internal class TransparencyBlockStruct
{
    internal required uint Size { get; init; }

    internal required uint PixelCount { get; init; }

    internal required IEnumerable<byte> Data { get; init; }

    internal static TransparencyBlockStruct FromBytes(BinaryReader br)
    {
        var size = br.ReadUInt32();
        var pixelCount = br.ReadUInt32();
        var data = br.ReadBytes(pixelCount * 3);

        return new TransparencyBlockStruct
        {
            Size = size,
            PixelCount = pixelCount,
            Data = data
        };
    }
}
