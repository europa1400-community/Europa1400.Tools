using Europa1400.Tools.Enums;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Sbf;

internal class SoundbankDefinitionStruct
{
    internal required uint Address { get; init; }

    internal required string Name { get; init; }

    internal required SoundbankType SoundbankType { get; init; }

    internal required IEnumerable<byte> Padding { get; init; }

    internal static SoundbankDefinitionStruct FromBytes(BinaryReader br)
    {
        var address = br.ReadUInt32();
        var name = br.ReadPaddedString(50);
        var soundbankType = (SoundbankType)br.ReadUInt16();
        var padding = br.ReadBytes(8);

        return new SoundbankDefinitionStruct
        {
            Address = address,
            Name = name,
            Padding = padding,
            SoundbankType = soundbankType
        };
    }
}
