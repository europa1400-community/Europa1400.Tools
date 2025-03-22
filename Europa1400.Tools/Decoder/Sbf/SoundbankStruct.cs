using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Sbf
{
    internal class SoundbankStruct
    {
        internal required SoundbankDefinitionStruct SoundbankDefinition { get; init; }
        internal SoundbankHeaderStruct? SoundbankHeader { get; init; }
        internal required uint SoundCount { get; init; }
        internal required SoundDefinitionStruct[] SoundDefinitions { get; init; }
        internal required byte[][] Sounds { get; init; }

        internal static SoundbankStruct FromBytes(BinaryReader br, SoundbankDefinitionStruct soundbankDefinition)
        {
            var soundbankHeader = soundbankDefinition.SoundbankType switch
            {
                Enums.SoundbankType.Multi => SoundbankHeaderStruct.FromBytes(br),
                _ => null
            };
            var soundCount = soundbankDefinition.SoundbankType switch
            {
                Enums.SoundbankType.Single => (uint)1,
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