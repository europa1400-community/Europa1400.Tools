using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Sbf
{
    public class SbfStruct
    {
        public string Name { get; private set; }
        public uint SoundbankCount { get; private set; }
        public byte[] MagicBytes { get; private set; }
        public byte[] Padding { get; private set; }
        public SoundbankDefinitionStruct[] SoundbankDefinitions { get; private set; }
        public SoundbankStruct[] Soundbanks { get; private set; }

        public static SbfStruct FromBytes(BinaryReader br)
        {
            var name = br.ReadPaddedString(308);
            var soundbankCount = br.ReadUInt32();
            var magicBytes = br.ReadBytes(4);
            var padding = br.ReadBytes(8);
            var soundbankDefinitions = br.ReadArray(SoundbankDefinitionStruct.FromBytes, soundbankCount);
            var soundbanks = br.ReadArray((reader, idx) => SoundbankStruct.FromBytes(reader, soundbankDefinitions[idx]),
                soundbankCount);

            return new SbfStruct
            {
                MagicBytes = magicBytes,
                Name = name,
                Padding = padding,
                SoundbankCount = soundbankCount,
                SoundbankDefinitions = soundbankDefinitions,
                Soundbanks = soundbanks
            };
        }
    }
}