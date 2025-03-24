namespace Europa1400.Tools.Structs.Sbf;

public class SoundbankHeaderStruct
{
    public required uint SoundCount { get; init; }
    public required uint Unknown1 { get; init; }
    public required uint Unknown2 { get; init; }

    public static SoundbankHeaderStruct FromBytes(BinaryReader br)
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