using System.IO;
using System.IO.Compression;

namespace Europa1400.Tools.Decoder.Sav;

internal class SavStruct
{
    internal required long CompressedSize { get; init; }
    internal required long DecompressedSize { get; init; }
    internal required byte[] DecompressedData { get; init; }

    internal static SavStruct FromBytes(BinaryReader br)
    {
        var compressedSize = br.BaseStream.Length;
        using var gzipStream = new GZipStream(br.BaseStream, CompressionMode.Decompress, leaveOpen: true);
        using var memoryStream = new MemoryStream();
        gzipStream.CopyTo(memoryStream);
        var decompressedData = memoryStream.ToArray();

        return new SavStruct
        {
            CompressedSize = compressedSize,
            DecompressedSize = decompressedData.Length,
            DecompressedData = decompressedData
        };
    }
}
