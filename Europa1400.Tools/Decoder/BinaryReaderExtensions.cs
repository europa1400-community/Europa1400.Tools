namespace Europa1400.Tools.Decoder;

using System.IO;
using System.Text;

public static class BinaryReaderExtensions
{
    public static string ReadString(this BinaryReader reader, int length)
    {
        var bytes = reader.ReadBytes(length);
        return Encoding.Latin1.GetString(bytes);
    }
}
