using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Ageb
{
    public class AgebBuildingStruct
    {
        public byte GroupId { get; set; }
        public string Name { get; set; }
        public byte Unknown1 { get; set; }
        public byte SizeData { get; set; }
        public ushort[] UnknownData1 { get; set; }
        public uint[] UnknownData2 { get; set; }
        public byte[] UnknownData3 { get; set; }
        public byte[] UnknownData4 { get; set; }
        public byte[] UnknownData5 { get; set; }
        public AgebBuildingCoordinatesStruct Coordinates1 { get; set; }
        public AgebBuildingCoordinatesStruct Coordinates2 { get; set; }
        public uint Time { get; set; }
        public byte Level { get; set; }
        public byte Unknown2 { get; set; }
        public uint Price { get; set; }

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