using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Bgf;

internal class BgfSkeletonStruct
{
    internal required string Name { get; init; }
    internal required Vector3Struct Vector1 { get; init; }
    internal required byte Unknown1 { get; init; }
    internal required Vector3Struct Vector2 { get; init; }

    internal static BgfSkeletonStruct FromBytes(BinaryReader br)
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
