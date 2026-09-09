// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Europa1400.Tools.Extensions;

public static class BinaryReaderExtensions
{
    public static string ReadString(this BinaryReader reader, int length)
    {
        var bytes = reader.ReadBytes(length);
        var latin1 = Encoding.GetEncoding(28591);

        if (latin1 == null)
            throw new NullReferenceException("Latin1 encoding not found.");

        return latin1.GetString(bytes);
    }

    public static byte PeekByte(this BinaryReader reader)
    {
        if (reader.BaseStream.Position == reader.BaseStream.Length) throw new EndOfStreamException();

        var value = reader.ReadByte();
        reader.BaseStream.Seek(-1, SeekOrigin.Current);
        return value;
    }

    public static int SkipWhile(this BinaryReader reader, byte value)
    {
        var count = 0;

        while (reader.BaseStream.Position < reader.BaseStream.Length && reader.PeekByte() == value)
        {
            reader.ReadByte();
            count++;
        }

        return count;
    }

    public static bool SkipOptionalByte(this BinaryReader reader, byte value)
    {
        var peekedValue = reader.PeekByte();

        if (peekedValue != value) return false;

        reader.ReadByte();
        return true;
    }

    public static bool SkipOptionalBytesAll(this BinaryReader reader, params byte[] values)
    {
        var initialPosition = reader.BaseStream.Position;

        if (Array.TrueForAll(values, reader.SkipOptionalByte)) return true;

        reader.BaseStream.Position = initialPosition;
        return false;
    }

    public static string ReadCString(this BinaryReader reader)
    {
        var byteList = new List<byte>();
        byte b;

        while ((b = reader.ReadByte()) != 0) byteList.Add(b);

        return Encoding.GetEncoding("ISO-8859-1").GetString(byteList.ToArray());
    }

    public static string ReadPaddedString(this BinaryReader reader, int length)
    {
        var str = reader.ReadString(length);

        return str.Replace("\0", "");
    }

    public static void SkipRequiredByte(this BinaryReader reader, byte value)
    {
        var peekedValue = reader.PeekByte();

        if (peekedValue != value) throw new InvalidDataException($"Expected byte {value} but got {peekedValue}.");

        reader.ReadByte();
    }

    internal static void SkipRequiredBytes(this BinaryReader reader, params byte[] values)
    {
        foreach (var value in values) reader.SkipRequiredByte(value);
    }

    public static uint[] ReadPaddedUInt32Array(this BinaryReader reader, int readCount, int padCount)
    {
        var result = new uint[padCount];

        for (var i = 0; i < padCount; i++) result[i] = reader.ReadUInt32();

        return result;
    }

    public static void Skip(this BinaryReader reader, int count)
    {
        reader.BaseStream.Seek(count, SeekOrigin.Current);
    }

    public static void SkipUntilInclusive(this BinaryReader reader, byte value)
    {
        try
        {
            var currentByte = reader.ReadByte();
            while (currentByte != value) currentByte = reader.ReadByte();
        }
        catch (EndOfStreamException)
        {
            throw new InvalidOperationException($"The value {value} was not found in the stream.");
        }
    }

    public static void SkipNonLatin1(this BinaryReader reader)
    {
        while (true)
        {
            var value = reader.ReadByte();

            if ((value < 0x20 || value >= 0x7F) && value < 0xA0) continue;

            reader.BaseStream.Seek(-1, SeekOrigin.Current);
            break;
        }
    }

    public static T[] ReadUntilException<T>(this BinaryReader br, Func<BinaryReader, T> readFunc,
        params Type[] exceptionTypes)
    {
        var list = new List<T>();

        while (true)
            try
            {
                list.Add(readFunc(br));
            }
            catch (Exception ex) when (Array.Exists(exceptionTypes, t => t.IsInstanceOfType(ex)))
            {
                break;
            }

        return list.ToArray();
    }

    internal static T[] ReadArray<T>(this BinaryReader br, Func<BinaryReader, T> readFunc, IConvertible? size)
    {
        if (size is null) return [];

        var list = new List<T>();
        var count = Convert.ToInt32(size);

        for (var i = 0; i < count; i++) list.Add(readFunc(br));

        return list.ToArray();
    }

    internal static T[] ReadArray<T>(this BinaryReader br, Func<BinaryReader, int, T> readFunc, IConvertible? size)
    {
        if (size is null) return [];

        var list = new List<T>();
        var count = Convert.ToInt32(size);

        for (var i = 0; i < count; i++) list.Add(readFunc(br, i));

        return list.ToArray();
    }

    internal static int[] ReadInt32s(this BinaryReader br, IConvertible? size)
    {
        if (size is null) return [];

        var count = Convert.ToInt32(size);
        var array = new int[count];

        for (var i = 0; i < count; i++) array[i] = br.ReadInt32();

        return array;
    }

    internal static float[] ReadSingles(this BinaryReader br, IConvertible? size)
    {
        if (size is null) return [];

        var count = Convert.ToInt32(size);
        var array = new float[count];

        for (var i = 0; i < count; i++) array[i] = br.ReadSingle();

        return array;
    }

    internal static uint[] ReadUInt32s(this BinaryReader br, IConvertible? size)
    {
        if (size is null) return [];

        var count = Convert.ToInt32(size);
        var array = new uint[count];

        for (var i = 0; i < count; i++) array[i] = br.ReadUInt32();

        return array;
    }

    internal static short[] ReadInt16s(this BinaryReader br, IConvertible? size)
    {
        if (size is null) return [];

        var count = Convert.ToInt32(size);
        var array = new short[count];

        for (var i = 0; i < count; i++) array[i] = br.ReadInt16();

        return array;
    }

    internal static ushort[] ReadUInt16s(this BinaryReader br, IConvertible? size)
    {
        if (size is null) return [];

        var count = Convert.ToInt32(size);
        var array = new ushort[count];

        for (var i = 0; i < count; i++) array[i] = br.ReadUInt16();

        return array;
    }

    internal static byte[] ReadBytes(this BinaryReader br, IConvertible? size)
    {
        if (size is null) return [];

        var count = Convert.ToInt32(size);
        return br.ReadBytes(count);
    }
}