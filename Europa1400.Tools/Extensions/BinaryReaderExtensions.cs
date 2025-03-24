// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Europa1400.Tools.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static string ReadString(this BinaryReader reader, int length)
        {
            var bytes = reader.ReadBytes(length);
            return Encoding.ASCII.GetString(bytes);
        }

        public static byte PeekByte(this BinaryReader reader)
        {
            if (reader.BaseStream.Position == reader.BaseStream.Length) throw new EndOfStreamException();

            var value = reader.ReadByte();
            reader.BaseStream.Seek(-1, SeekOrigin.Current);
            return value;
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
            var sb = new StringBuilder();
            char c;

            while ((c = reader.ReadChar()) != '\0') sb.Append(c);

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

            if (peekedValue != value) throw new InvalidDataException($"Expected byte {value} but got {peekedValue}.");

            reader.ReadByte();
        }

        public static void SkipRequiredBytes(this BinaryReader reader, params byte[] values)
        {
            foreach (var value in values) reader.SkipRequiredByte(value);
        }

        public static void SkipRequiredBytes(this BinaryReader reader, byte value, int length)
        {
            for (var i = 0; i < length; i++) reader.SkipRequiredByte(value);
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

        public static T[] ReadUntilException<T>(this BinaryReader br, Func<BinaryReader, int, T> readFunc,
            params Type[] exceptionTypes)
        {
            var list = new List<T>();
            var idx = 0;

            while (true)
                try
                {
                    list.Add(readFunc(br, idx));
                    idx++;
                }
                catch (Exception ex) when (Array.Exists(exceptionTypes, t => t.IsInstanceOfType(ex)))
                {
                    break;
                }

            return list.ToArray();
        }

        public static T[] ReadArray<T>(this BinaryReader br, Func<BinaryReader, T> readFunc, IConvertible? size)
        {
            if (size is null) return Array.Empty<T>();

            var list = new List<T>();
            var count = Convert.ToInt32(size);

            for (var i = 0; i < count; i++) list.Add(readFunc(br));

            return list.ToArray();
        }

        public static T[] ReadArray<T>(this BinaryReader br, Func<BinaryReader, int, T> readFunc, IConvertible? size)
        {
            if (size is null) return Array.Empty<T>();

            var list = new List<T>();
            var count = Convert.ToInt32(size);

            for (var i = 0; i < count; i++) list.Add(readFunc(br, i));

            return list.ToArray();
        }

        public static int[] ReadInt32s(this BinaryReader br, IConvertible? size)
        {
            if (size is null) return Array.Empty<int>();

            var count = Convert.ToInt32(size);
            var array = new int[count];

            for (var i = 0; i < count; i++) array[i] = br.ReadInt32();

            return array;
        }

        public static uint[] ReadUInt32s(this BinaryReader br, IConvertible? size)
        {
            if (size is null) return Array.Empty<uint>();

            var count = Convert.ToInt32(size);
            var array = new uint[count];

            for (var i = 0; i < count; i++) array[i] = br.ReadUInt32();

            return array;
        }

        public static short[] ReadInt16s(this BinaryReader br, IConvertible? size)
        {
            if (size is null) return Array.Empty<short>();

            var count = Convert.ToInt32(size);
            var array = new short[count];

            for (var i = 0; i < count; i++) array[i] = br.ReadInt16();

            return array;
        }

        public static ushort[] ReadUInt16s(this BinaryReader br, IConvertible? size)
        {
            if (size is null) return Array.Empty<ushort>();

            var count = Convert.ToInt32(size);
            var array = new ushort[count];

            for (var i = 0; i < count; i++) array[i] = br.ReadUInt16();

            return array;
        }

        public static byte[] ReadBytes(this BinaryReader br, IConvertible? size)
        {
            if (size is null) return Array.Empty<byte>();

            var count = Convert.ToInt32(size);
            return br.ReadBytes(count);
        }
    }
}