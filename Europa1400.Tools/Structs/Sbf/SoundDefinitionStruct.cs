using System.IO;

namespace Europa1400.Tools.Structs.Sbf
{
    public class SoundDefinitionStruct
    {
        public SoundType SoundType { get; private set; }
        public uint Length { get; private set; }
        public uint Magic { get; private set; }

        public static SoundDefinitionStruct FromBytes(BinaryReader br)
        {
            var soundType = (SoundType)br.ReadUInt32();
            var length = br.ReadUInt32();
            var magic = br.ReadUInt32();

            return new SoundDefinitionStruct
            {
                SoundType = soundType,
                Length = length,
                Magic = magic
            };
        }
    }
}