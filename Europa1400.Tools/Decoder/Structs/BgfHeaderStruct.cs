namespace Europa1400.Tools.Decoder.Structs;

public class BgfHeaderStruct
{
    public required string Name { get; init; }
    public int MappingAddress { get; init; }
    public byte Unknown1 { get; init; }
    public byte Unknown2 { get; init; }
    public int? SkeletonCount { get; init; }
    public int TextureCount { get; init; }

    public static BgfHeaderStruct FromBytes(BinaryReader br)
    {
        var name = br.ReadCString();
        br.SkipRequiredByte(0x2E);
        var mappingAddress = br.ReadInt32();
        br.SkipRequiredBytes(0x01, 0x01);
        var unknown1 = br.ReadByte();
        br.SkipRequiredBytes(0xCD, 0xAB, 0x02);
        var unknown2 = br.ReadByte();
        var wasSkipped1 = br.SkipOptionalByte(0x37);
        var skeletonCount = wasSkipped1 ? br.ReadInt32() as int? : null;
        br.SkipRequiredBytes(0x03, 0x04);
        var textureCount = br.ReadInt32();

        return new BgfHeaderStruct
        {
            Name = name,
            MappingAddress = mappingAddress,
            Unknown1 = unknown1,
            Unknown2 = unknown2,
            SkeletonCount = skeletonCount,
            TextureCount = textureCount
        };
    }
}


