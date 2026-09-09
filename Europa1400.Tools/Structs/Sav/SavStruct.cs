using System.IO;
using System.IO.Compression;

namespace Europa1400.Tools.Structs.Sav;

public class SavStruct
{
    public required long CompressedSize { get; init; }
    public required long DecompressedSize { get; init; }
    public required byte[] DecompressedData { get; init; }

    public static SavStruct FromBytes(BinaryReader br)
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
