using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Structs;

public class AobjObjectStruct
{
    public byte Type { get; init; }
    public required string Name { get; init; }
    public byte Level { get; init; }
    public int Time { get; init; }
    public required IEnumerable<short> UnknownData1 { get; init; }
    public required IEnumerable<short> UnknownData2 { get; init; }
    public short Unknown1 { get; init; }
    public int Price { get; init; }
    public int Unknown2 { get; init; }

    public static AobjObjectStruct FromBytes(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        try
        {
            var type = reader.ReadByte();
            var name = reader.ReadString(32);
            var level = reader.ReadByte();
            var time = reader.ReadInt32();
            var unknownData1 = reader.ReadInt16s(4);
            var unknownData2 = reader.ReadInt16s(4);
            var unknown1 = reader.ReadInt16();
            var price = reader.ReadInt32();
            var unknown2 = reader.ReadInt32();

            return new AobjObjectStruct
            {
                Type = type,
                Name = name,
                Level = level,
                Time = time,
                UnknownData1 = unknownData1,
                UnknownData2 = unknownData2,
                Unknown1 = unknown1,
                Price = price,
                Unknown2 = unknown2
            };

        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while decoding the data.", ex);
        }
    }
}

