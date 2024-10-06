using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Sbf;

internal class SbfStruct
{
    public required string Name { get; init; }

    public required int SoundbankCount { get; init; }

    public required byte[] MagicBytes { get; init; }

    public required byte[] Padding { get; init; }

    public required SoundbankDefinitionStruct[] SoundbankDefinitions { get; init; }

    public required SoundbankStruct[] Soundbanks { get; init; }

    public static SbfStruct FromBytes(BinaryReader br)
    {
        var name = br.ReadPaddedString(308);
        var soundbankCount = br.ReadInt32();
        var magicBytes = br.ReadBytes(4);
        var padding = br.ReadBytes(8);
        var soundbankDefinitions = Enumerable.Range(0, soundbankCount).Select(_ => SoundbankDefinitionStruct.FromBytes(br)).ToArray();
        var soundbanks = Enumerable.Range(0, soundbankCount).Select(idx => SoundbankStruct.FromBytes(br, soundbankDefinitions[idx])).ToArray();

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
