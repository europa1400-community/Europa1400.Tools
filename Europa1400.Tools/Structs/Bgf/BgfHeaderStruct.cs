using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfHeaderStruct
    {
        public string Name { get; private set; }
        public uint MappingAddress { get; private set; }
        public byte Unknown1 { get; private set; }
        public byte Unknown2 { get; private set; }
        public uint? SkeletonCount { get; private set; }
        public uint TextureCount { get; private set; }

        public static BgfHeaderStruct FromBytes(BinaryReader br)
        {
            var name = br.ReadCString();
            br.SkipRequiredByte(0x2E);
            var mappingAddress = br.ReadUInt32();
            br.SkipRequiredBytes(0x01, 0x01);
            var unknown1 = br.ReadByte();
            br.SkipRequiredBytes(0xCD, 0xAB, 0x02);
            var unknown2 = br.ReadByte();
            var wasSkipped1 = br.SkipOptionalByte(0x37);
            var skeletonCount = wasSkipped1 ? br.ReadUInt32() as uint? : null;
            br.SkipRequiredBytes(0x03, 0x04);
            var textureCount = br.ReadUInt32();

            return new BgfHeaderStruct
            {
                Name = name,
                MappingAddress = mappingAddress,
                Unknown1 = unknown1,
                Unknown2 = unknown2,
                SkeletonCount = skeletonCount,
                TextureCount = textureCount
            };
        }
    }
}