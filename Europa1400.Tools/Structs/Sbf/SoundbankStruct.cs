using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Sbf
{
    public class SoundbankStruct
    {
        public SoundbankDefinitionStruct SoundbankDefinition { get; set; }
        public SoundbankHeaderStruct? SoundbankHeader { get; set; }
        public uint SoundCount { get; set; }
        public SoundDefinitionStruct[] SoundDefinitions { get; set; }
        public byte[][] Sounds { get; set; }

        public static SoundbankStruct FromBytes(BinaryReader br, SoundbankDefinitionStruct soundbankDefinition)
        {
            var soundbankHeader = soundbankDefinition.SoundbankType switch
            {
                SoundbankType.Multi => SoundbankHeaderStruct.FromBytes(br),
                _ => null
            };
            var soundCount = soundbankDefinition.SoundbankType switch
            {
                SoundbankType.Single => (uint)1,
                _ => soundbankHeader!.SoundCount
            };
            var soundDefinitions = br.ReadArray(SoundDefinitionStruct.FromBytes, soundCount);
            var sounds = br.ReadArray((reader, idx) => reader.ReadBytes(soundDefinitions[idx].Length), soundCount);

            return new SoundbankStruct
            {
                SoundbankDefinition = soundbankDefinition,
                SoundbankHeader = soundbankHeader,
                SoundCount = soundCount,
                Sounds = sounds,
                SoundDefinitions = soundDefinitions
            };
        }
    }
}