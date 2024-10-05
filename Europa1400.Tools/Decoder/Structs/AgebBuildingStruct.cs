using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Structs;

public class AgebBuildingStruct
{
    public byte GroupId { get; init; }
    public required string Name { get; init; }
    public byte Unknown1 { get; init; }
    public required IEnumerable<short> UnkownData1 { get; init; }
    public required IEnumerable<short> UnknownData2 { get; init; }
    public required IEnumerable<byte> UnknownData3 { get; init; }
    public required IEnumerable<byte> UnknownData4 { get; init; }
    public required IEnumerable<byte> UnknownData5 { get; init; }
    public required AgebBuildingCoordinatesStruct Coordinates1 { get; init; }
    public required AgebBuildingCoordinatesStruct Coordinates2 { get; init; }
    public int Time { get; init; }
    public byte Level { get; init; }
    public byte Unknown2 { get; init; }
    public int Price { get; init; }

    public static AgebBuildingStruct FromBytes(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        try
        {
            var groupId = reader.ReadByte();
            var name = reader.ReadString(32);
            var unknown1 = reader.ReadByte();
            var sizeData = reader.ReadByte();
            var data1 = reader.ReadInt16s(136);
            var data2 = reader.ReadInt16s(128);
            var data3 = reader.ReadBytes(65);
            var data4 = reader.ReadBytes(63);
            var data5 = reader.ReadBytes(26);
            var coordinates1 = AgebBuildingCoordinatesStruct.FromBytes(reader.ReadBytes(3));
            var coordinates2 = AgebBuildingCoordinatesStruct.FromBytes(reader.ReadBytes(3));
            var time = reader.ReadInt32();
            var level = reader.ReadByte();
            var unknown2 = reader.ReadByte();
            var price = reader.ReadInt32();

            return new AgebBuildingStruct
            {
                GroupId = groupId,
                Name = name,
                Unknown1 = unknown1,
                UnkownData1 = data1,
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
        catch (Exception ex)
        {
            throw new Exception("An error occurred while decoding the data.", ex);
        }
    }
}
