using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Sbf
{
    public class SoundbankStruct
    {
        public SoundbankDefinitionStruct SoundbankDefinition { get; private set; }
        public SoundbankHeaderStruct? SoundbankHeader { get; private set; }
        public uint SoundCount { get; private set; }
        public SoundDefinitionStruct[] SoundDefinitions { get; private set; }
        public byte[][] Sounds { get; private set; }

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