using System.IO;

namespace Europa1400.Tools.Structs.Sbf
{
    public class SoundbankHeaderStruct
    {
        public uint SoundCount { get; private set; }
        public uint Unknown1 { get; private set; }
        public uint Unknown2 { get; private set; }

        public static SoundbankHeaderStruct FromBytes(BinaryReader br)
        {
            var soundCount = br.ReadUInt32();
            var magic1 = br.ReadUInt32();
            var magic2 = br.ReadUInt32();

            return new SoundbankHeaderStruct
            {
                SoundCount = soundCount,
                Unknown1 = magic1,
                Unknown2 = magic2
            };
        }
    }
}