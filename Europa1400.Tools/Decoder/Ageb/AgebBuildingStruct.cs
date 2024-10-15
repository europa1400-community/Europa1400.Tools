using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Ageb;

internal class AgebBuildingStruct
{
    internal required byte GroupId { get; init; }
    internal required string Name { get; init; }
    internal required byte Unknown1 { get; init; }
    internal required ushort[] UnknownData1 { get; init; }
    internal required uint[] UnknownData2 { get; init; }
    internal required byte[] UnknownData3 { get; init; }
    internal required byte[] UnknownData4 { get; init; }
    internal required byte[] UnknownData5 { get; init; }
    internal required AgebBuildingCoordinatesStruct Coordinates1 { get; init; }
    internal required AgebBuildingCoordinatesStruct Coordinates2 { get; init; }
    internal required uint Time { get; init; }
    internal required byte Level { get; init; }
    internal required byte Unknown2 { get; init; }
    internal required uint Price { get; init; }

    internal static AgebBuildingStruct FromBytes(BinaryReader br)
    {
        var groupId = br.ReadByte();
        var name = br.ReadPaddedString(32);
        var unknown1 = br.ReadByte();
        var sizeData = br.ReadByte();
        var data1 = br.ReadUInt16s(68);
        var data2 = br.ReadUInt32s(62);
        var data3 = br.ReadBytes(65);
        var data4 = br.ReadBytes(63);
        var data5 = br.ReadBytes(26);
        var coordinates1 = AgebBuildingCoordinatesStruct.FromBytes(br);
        var coordinates2 = AgebBuildingCoordinatesStruct.FromBytes(br);
        var time = br.ReadUInt32();
        var level = br.ReadByte();
        var unknown2 = br.ReadByte();
        var price = br.ReadUInt32();

        return new AgebBuildingStruct
        {
            GroupId = groupId,
            Name = name,
            Unknown1 = unknown1,
            UnknownData1 = data1,
            UnknownData2 = data2,
            UnknownData3 = data3,
            UnknownData4 = data4,
            UnknownData5 = data5,
            Coordinates1 = coordinates1,
            Coordinates2 = coordinates2,
            Time = time,
            Level = level,
            Unknown2 = unknown2,
            Price = price
        };
    }
}
