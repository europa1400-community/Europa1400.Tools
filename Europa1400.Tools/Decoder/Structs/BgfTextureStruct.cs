namespace Europa1400.Tools.Decoder.Structs;

public class BgfTextureStruct
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? NameAppendix { get; init; }
    public byte? Unknown1 { get; init; }
    public byte? Unknown2 { get; init; }

    public static BgfTextureStruct FromBytes(BinaryReader br)
    {
        br.SkipRequiredBytes(0x05, 0x06);
        var id = br.ReadInt32();
        br.SkipOptionalByte(0x07);
        br.SkipOptionalByte(0x08);
        var name = br.ReadCString();
        var wasSkipped1 = br.SkipOptionalByte(0x08);
        var wasSkipped2 = br.SkipOptionalByte(0x09);
        var nameAppendix = wasSkipped1 || wasSkipped2 ? br.ReadCString() : null;
        var wasSkipped3 = br.SkipOptionalByte(0x0A);
        var unknown1 = wasSkipped3 ? br.ReadByte() as byte? : null;
        var wasSkipped4 = br.SkipOptionalByte(0x0B);
        var unknown2 = wasSkipped4 ? br.ReadByte() as byte? : null;
        br.SkipUntilInclusive(0x28);


        return new BgfTextureStruct
        {
            Id = id,
            Name = name,
            NameAppendix = nameAppendix,
            Unknown1 = unknown1,
            Unknown2 = unknown2
        };
    }
}
