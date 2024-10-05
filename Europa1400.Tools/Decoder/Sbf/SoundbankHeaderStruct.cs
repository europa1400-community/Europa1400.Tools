namespace Europa1400.Tools.Decoder.Sbf;

internal class SoundbankHeaderStruct
{
    public required uint SoundCount { get; init; }

    public required uint Magic1 { get; init; }

    public required uint Magic2 { get; init; }

    internal static SoundbankHeaderStruct FromBytes(BinaryReader br)
    {
        var soundCount = br.ReadUInt32();
        var magic1 = br.ReadUInt32();
        var magic2 = br.ReadUInt32();

        return new SoundbankHeaderStruct
        {
            SoundCount = soundCount,
            Magic1 = magic1,
            Magic2 = magic2
        };
    }
}
