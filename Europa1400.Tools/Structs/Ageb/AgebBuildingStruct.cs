using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Ageb
{
    public class AgebBuildingStruct
    {
        public byte GroupId { get; private set; }
        public string Name { get; private set; }
        public byte Unknown1 { get; private set; }
        public byte SizeData { get; private set; }
        public ushort[] UnknownData1 { get; private set; }
        public uint[] UnknownData2 { get; private set; }
        public byte[] UnknownData3 { get; private set; }
        public byte[] UnknownData4 { get; private set; }
        public byte[] UnknownData5 { get; private set; }
        public AgebBuildingCoordinatesStruct Coordinates1 { get; private set; }
        public AgebBuildingCoordinatesStruct Coordinates2 { get; private set; }
        public uint Time { get; private set; }
        public byte Level { get; private set; }
        public byte Unknown2 { get; private set; }
        public uint Price { get; private set; }

        public static AgebBuildingStruct FromBytes(BinaryReader br)
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
                SizeData = sizeData,
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
}