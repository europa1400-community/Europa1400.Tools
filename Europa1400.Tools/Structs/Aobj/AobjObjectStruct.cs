using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Aobj;

public class AobjObjectStruct
{
    public required byte Type { get; init; }
    public required string Name { get; init; }
    public required byte Level { get; init; }
    public required uint Time { get; init; }
    public required ushort[] UnknownData1 { get; init; }
    public required ushort[] UnknownData2 { get; init; }
    public required ushort Unknown1 { get; init; }
    public required uint Price { get; init; }
    public required uint Unknown2 { get; init; }
    public required byte Unknown3 { get; init; }

    public static AobjObjectStruct FromBytes(BinaryReader br)
    {
        var type = br.ReadByte();
        var name = br.ReadPaddedString(32);
        var level = br.ReadByte();
        var time = br.ReadUInt32();
        var unknownData1 = br.ReadUInt16s(4);
        var unknownData2 = br.ReadUInt16s(4);
        var unknown1 = br.ReadUInt16();
        var price = br.ReadUInt32();
        var unknown2 = br.ReadUInt32();
        var unknown3 = br.ReadByte();

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
            Unknown2 = unknown2,
            Unknown3 = unknown3
        };
    }
}