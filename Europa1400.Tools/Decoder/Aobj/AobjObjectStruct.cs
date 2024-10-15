using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Aobj;

internal class AobjObjectStruct
{
    internal required byte Type { get; init; }
    internal required string Name { get; init; }
    internal required byte Level { get; init; }
    internal required uint Time { get; init; }
    internal required ushort[] UnknownData1 { get; init; }
    internal required ushort[] UnknownData2 { get; init; }
    internal required ushort Unknown1 { get; init; }
    internal required uint Price { get; init; }
    internal required uint Unknown2 { get; init; }
    internal required byte Unknown3 { get; init; }

    internal static AobjObjectStruct FromBytes(BinaryReader br)
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

