using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Aobj
{
    public class AobjObjectStruct
    {
        public byte Type { get; set; }
        public string Name { get; set; }
        public byte Level { get; set; }
        public uint Time { get; set; }
        public ushort[] UnknownData1 { get; set; }
        public ushort[] UnknownData2 { get; set; }
        public ushort Unknown1 { get; set; }
        public uint Price { get; set; }
        public uint Unknown2 { get; set; }
        public byte Unknown3 { get; set; }

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
}