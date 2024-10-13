namespace Europa1400.Tools.Extensions;

using System.IO;
using System.Text;

public static class BinaryReaderExtensions
{
    public static string ReadString(this BinaryReader reader, int length)
    {
        var bytes = reader.ReadBytes(length);
        return Encoding.Latin1.GetString(bytes);
    }

    public static byte PeekByte(this BinaryReader reader)
    {
        if (reader.BaseStream.Position == reader.BaseStream.Length)
        {
            throw new EndOfStreamException();
        }

        var value = reader.ReadByte();
        reader.BaseStream.Seek(-1, SeekOrigin.Current);
        return value;
    }

    public static bool SkipOptionalByte(this BinaryReader reader, byte value)
    {
        var peekedValue = reader.PeekByte();

        if (peekedValue == value)
        {
            reader.ReadByte();
            return true;
        }

        return false;
    }

    public static bool SkipOptionalBytesAll(this BinaryReader reader, params byte[] values)
    {
        var initialPosition = reader.BaseStream.Position;

        foreach (var value in values)
        {
            if (!reader.SkipOptionalByte(value))
            {
                reader.BaseStream.Position = initialPosition;
                return false;
            }
        }

        return true;
    }

    public static string ReadCString(this BinaryReader reader)
    {
        var sb = new StringBuilder();
        char c;

        while ((c = reader.ReadChar()) != '\0')
        {
            sb.Append(c);
        }

        return sb.ToString();
    }

    public static string ReadPaddedString(this BinaryReader reader, int length)
    {
        var str = reader.ReadString(length);

        return str.Replace("\0", "");
    }

    public static void SkipRequiredByte(this BinaryReader reader, byte value)
    {
        var peekedValue = reader.PeekByte();

        if (peekedValue != value)
        {
            throw new InvalidDataException($"Expected byte {value} but got {peekedValue}.");
        }

        reader.ReadByte();
    }

    public static void SkipRequiredBytes(this BinaryReader reader, params byte[] values)
    {
        foreach (var value in values)
        {
            reader.SkipRequiredByte(value);
        }
    }

    public static IEnumerable<ushort> ReadUInt16s(this BinaryReader reader, int count)
    {
        var result = new ushort[count];

        for (var i = 0; i < count; i++)
        {
            result[i] = reader.ReadUInt16();
        }

        return result;
    }

    public static IEnumerable<uint> ReadUInt32s(this BinaryReader reader, int count)
    {
        var result = new uint[count];

        for (var i = 0; i < count; i++)
        {
            result[i] = reader.ReadUInt32();
        }

        return result;
    }

    public static IEnumerable<uint> ReadPaddedUInt32Array(this BinaryReader reader, int readCount, int padCount)
    {
        var result = new uint[readCount > padCount ? readCount : padCount];

        for (var i = 0; i < readCount; i++)
        {
            result[i] = reader.ReadUInt32();
        }

        return result;
    }

    public static void Skip(this BinaryReader reader, int count)
    {
        reader.BaseStream.Seek(count, SeekOrigin.Current);
    }

    public static void SkipUntilInclusive(this BinaryReader reader, byte value)
    {
        while (reader.ReadByte() != value) { }
    }

    public static void SkipNonLatin1(this BinaryReader reader)
    {
        while (true)
        {
            var value = reader.ReadByte();

            if (value >= 0x20 && value < 0x7F || value >= 0xA0)
            {
                reader.BaseStream.Seek(-1, SeekOrigin.Current);
                break;
            }
        }
    }

    public static List<T> ReadUntilException<T>(this BinaryReader br, Func<BinaryReader, T> readFunc, params Type[] exceptionTypes)
    {
        var list = new List<T>();

        while (true)
        {
            try
            {
                list.Add(readFunc(br));
            }
            catch (Exception ex) when (exceptionTypes.Any(t => t.IsInstanceOfType(ex)))
            {
                break;
            }
        }

        return list;
    }

    public static IEnumerable<T> ReadArray<T, TSize>(this BinaryReader br, Func<BinaryReader, int, T> readFunc, TSize size) where TSize : IConvertible
    {
        var count = Convert.ToInt32(size);
        for (var i = 0; i < count; i++)
        {
            yield return readFunc(br, i);
        }
    }

    public static IEnumerable<T>? ReadArray<T, TSize>(this BinaryReader br, Func<BinaryReader, int, T> readFunc, TSize? size) where TSize : struct, IConvertible => ReadArray(br, readFunc, size) ?? null;

    public static IEnumerable<T> ReadArray<T, TSize>(this BinaryReader br, Func<BinaryReader, T> readFunc, TSize size) where TSize : IConvertible
    {
        var count = Convert.ToInt32(size);
        for (var i = 0; i < count; i++)
        {
            yield return readFunc(br);
        }
    }

    public static IEnumerable<T>? ReadArray<T, TSize>(this BinaryReader br, Func<BinaryReader, T> readFunc, TSize? size) where TSize : struct, IConvertible => ReadArray(br, readFunc, size) ?? null;


    public static IEnumerable<int> ReadInt32s<TSize>(this BinaryReader br, TSize size) where TSize : IConvertible
    {
        var count = Convert.ToInt32(size);
        for (var i = 0; i < count; i++)
        {
            yield return br.ReadInt32();
        }
    }

    public static IEnumerable<int>? ReadInt32s<TSize>(this BinaryReader br, TSize? size) where TSize : struct, IConvertible => ReadInt32s(br, size) ?? null;

    public static IEnumerable<uint> ReadUInt32s<TSize>(this BinaryReader br, TSize size) where TSize : IConvertible
    {
        var count = Convert.ToInt32(size);
        for (var i = 0; i < count; i++)
        {
            yield return br.ReadUInt32();
        }
    }

    public static IEnumerable<uint>? ReadUInt32s<TSize>(this BinaryReader br, TSize? size) where TSize : struct, IConvertible => ReadUInt32s(br, size) ?? null;

    public static IEnumerable<short> ReadInt16s<TSize>(this BinaryReader br, TSize size) where TSize : IConvertible
    {
        var count = Convert.ToInt32(size);
        for (var i = 0; i < count; i++)
        {
            yield return br.ReadInt16();
        }
    }

    public static IEnumerable<short>? ReadInt16s<TSize>(this BinaryReader br, TSize? size) where TSize : struct, IConvertible => ReadInt16s(br, size) ?? null;

    public static IEnumerable<ushort> ReadUInt16s<TSize>(this BinaryReader br, TSize size) where TSize : IConvertible
    {
        var count = Convert.ToInt32(size);
        for (var i = 0; i < count; i++)
        {
            yield return br.ReadUInt16();
        }
    }

    public static IEnumerable<ushort>? ReadUInt16s<TSize>(this BinaryReader br, TSize? size) where TSize : struct, IConvertible => ReadUInt16s(br, size) ?? null;

    public static IEnumerable<byte> ReadBytes<TSize>(this BinaryReader br, TSize size) where TSize : IConvertible
    {
        var count = Convert.ToInt32(size);
        for (var i = 0; i < count; i++)
        {
            yield return br.ReadByte();
        }
    }
}
