using System.IO;
using System.Text;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Ogr;

public class OgrStruct
{
    public required string Name { get; init; }
    public required byte[] RawData { get; init; }

    public static OgrStruct FromBytes(BinaryReader br)
    {
        var magic = br.ReadUInt32();
        if (magic != 0x3A6C00BB)
            throw new InvalidDataException($"Invalid OGR magic: expected 0x3A6C00BB, got 0x{magic:X8}");

        br.ReadUInt32(); // version, typically 1

        var nameLength = br.ReadByte();
        var name = Encoding.ASCII.GetString(br.ReadBytes(nameLength)).TrimEnd('\0');

        var rawData = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));

        return new OgrStruct
        {
            Name = name,
            RawData = rawData
        };
    }
}
