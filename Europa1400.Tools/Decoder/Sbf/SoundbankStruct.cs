using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Sbf
{
    internal class SoundbankStruct
    {
        public required SoundbankDefinitionStruct SoundbankDefinition { get; init; }

        public SoundbankHeaderStruct? SoundbankHeader { get; init; }

        public required uint SoundCount { get; init; }

        public required SoundDefinitionStruct[] SoundDefinitions { get; init; }

        public required byte[][] Sounds { get; init; }

        internal static SoundbankStruct FromBytes(BinaryReader br, SoundbankDefinitionStruct soundbankDefinition)
        {
            SoundbankHeaderStruct? soundbankHeader = null;
            
            if(soundbankDefinition.SoundbankType == Enums.SoundbankType.Multi)
                soundbankHeader = SoundbankHeaderStruct.FromBytes(br);

            var soundCount = soundbankDefinition.SoundbankType == Enums.SoundbankType.Single ? 1 : soundbankHeader!.SoundCount;
            var soundDefinitions = Enumerable.Range(0, (int)soundCount).Select(_ => SoundDefinitionStruct.FromBytes(br)).ToArray();
            var sounds = Enumerable.Range(0, (int)soundCount).Select(idx => br.ReadBytes(soundDefinitions[idx].Length)).ToArray();

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