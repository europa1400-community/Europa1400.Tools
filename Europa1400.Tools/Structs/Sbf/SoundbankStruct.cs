using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Sbf;

public class SoundbankStruct
{
    public required SoundbankDefinitionStruct SoundbankDefinition { get; init; }
    public SoundbankHeaderStruct? SoundbankHeader { get; init; }
    public required uint SoundCount { get; init; }
    public required SoundDefinitionStruct[] SoundDefinitions { get; init; }
    public required byte[][] Sounds { get; init; }

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