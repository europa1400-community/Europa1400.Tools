namespace Europa1400.Tools.Decoder;

public static class AgebDecoder
{
    public class BuildingStruct()
    {
        public byte GroupId { get; init; }
        public required string Name { get; init; }
        public byte Unknown1 { get; init; }
        public required char[] UnkownData1 { get; init; }
        public required char[] UnknownData2 { get; init; }
        public required byte[] UnknownData3 { get; init; }
        public required byte[] UnknownData4 { get; init; }
        public required byte[] UnknownData5 { get; init; }
        public required BuildingCoordinatesStruct Coordinates1 { get; init; }
        public required BuildingCoordinatesStruct Coordinates2 { get; init; }
        public int Time { get; init; }
        public byte Level { get; init; }
        public byte Unknown2 { get; init; }
        public int Price { get; init; }
    }

    public class BuildingCoordinatesStruct()
    {
        public byte X { get; init; }
        public byte Y { get; init; }
        public byte Z { get; init; }
    }

    public static BuildingCoordinatesStruct DecodeBuildingCoordinates(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        try
        {
            var x = reader.ReadByte();
            var y = reader.ReadByte();
            var z = reader.ReadByte();

            return new BuildingCoordinatesStruct
            {
                X = x,
                Y = y,
                Z = z
            };
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while decoding the data.", ex);
        }
    }

    public static BuildingStruct DecodeBuilding(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        try
        {
            var groupId = reader.ReadByte();
            var name = reader.ReadString(32);
            var unknown1 = reader.ReadByte();
            var sizeData = reader.ReadByte();
            var data1 = reader.ReadChars(136);
            var data2 = reader.ReadChars(128);
            var data3 = reader.ReadBytes(65);
            var data4 = reader.ReadBytes(63);
            var data5 = reader.ReadBytes(26);
            var coordinates1 = DecodeBuildingCoordinates(reader.ReadBytes(3));
            var coordinates2 = DecodeBuildingCoordinates(reader.ReadBytes(3));
            var time = reader.ReadInt32();
            var level = reader.ReadByte();
            var unknown2 = reader.ReadByte();
            var price = reader.ReadInt32();

            return new BuildingStruct
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
