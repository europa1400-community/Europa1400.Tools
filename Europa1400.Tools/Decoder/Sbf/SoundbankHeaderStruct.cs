namespace Europa1400.Tools.Decoder.Sbf;

internal class SoundbankHeaderStruct
{
    internal required uint SoundCount { get; init; }

    internal required uint Unknown1 { get; init; }

    internal required uint Unknown2 { get; init; }

    internal static SoundbankHeaderStruct FromBytes(BinaryReader br)
    {
        var soundCount = br.ReadUInt32();
        var magic1 = br.ReadUInt32();
        var magic2 = br.ReadUInt32();

        return new SoundbankHeaderStruct
        {
            SoundCount = soundCount,
            Unknown1 = magic1,
            Unknown2 = magic2
        };
    }
}
