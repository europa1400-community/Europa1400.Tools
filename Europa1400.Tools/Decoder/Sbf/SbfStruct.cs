using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Sbf;

internal class SbfStruct
{
    internal required string Name { get; init; }
    internal required uint SoundbankCount { get; init; }
    internal required byte[] MagicBytes { get; init; }
    internal required byte[] Padding { get; init; }
    internal required SoundbankDefinitionStruct[] SoundbankDefinitions { get; init; }
    internal required SoundbankStruct[] Soundbanks { get; init; }

    internal static SbfStruct FromBytes(BinaryReader br)
    {
        var name = br.ReadPaddedString(308);
        var soundbankCount = br.ReadUInt32();
        var magicBytes = br.ReadBytes(4);
        var padding = br.ReadBytes(8);
        var soundbankDefinitions = br.ReadArray(SoundbankDefinitionStruct.FromBytes, soundbankCount);
        var soundbanks = br.ReadArray((reader, idx) => SoundbankStruct.FromBytes(reader, soundbankDefinitions[idx]), soundbankCount);

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
