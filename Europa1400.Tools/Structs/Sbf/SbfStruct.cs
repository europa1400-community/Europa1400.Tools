using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Sbf;

public class SbfStruct
{
    public required string Name { get; init; }
    public required uint SoundbankCount { get; init; }
    public required byte[] MagicBytes { get; init; }
    public required byte[] Padding { get; init; }
    public required SoundbankDefinitionStruct[] SoundbankDefinitions { get; init; }
    public required SoundbankStruct[] Soundbanks { get; init; }

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