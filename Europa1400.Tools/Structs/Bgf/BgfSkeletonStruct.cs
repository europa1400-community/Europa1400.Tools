using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Bgf;

public class BgfSkeletonStruct
{
    public required string Name { get; init; }
    public required Vector3Struct Vector1 { get; init; }
    public required byte Unknown1 { get; init; }
    public required Vector3Struct Vector2 { get; init; }

    public static BgfSkeletonStruct FromBytes(BinaryReader br)
    {
        br.SkipRequiredByte(0x38);
        var name = br.ReadCString();
        br.SkipRequiredByte(0x39);
        var vector1 = Vector3Struct.FromBytes(br);
        var unknown1 = br.ReadByte();
        var vector2 = Vector3Struct.FromBytes(br);

        return new BgfSkeletonStruct
        {
            Name = name,
            Vector1 = vector1,
            Unknown1 = unknown1,
            Vector2 = vector2
        };
    }
}