using System.Runtime.Intrinsics;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Structs;

public class BgfSkeletonStruct
{
    public required string Name { get; init; }
    public required Vector3Struct Vector1 { get; init; }
    public byte Unknown1 { get; init; }
    public required Vector3Struct Vector2 { get; init; }

    public static BgfSkeletonStruct FromBytes(byte[] data)
    {
        var ms = new MemoryStream(data);
        var br = new BinaryReader(ms);

        return FromBytes(br);
    }

    public static BgfSkeletonStruct FromBytes(BinaryReader br)
    {
        br.SkipRequiredByte(0x38);
        var name = br.ReadCString();
        br.SkipRequiredByte(0x39);
        var vector1 = Vector3Struct.FromBytes(br.ReadBytes(12));
        var unknown1 = br.ReadByte();
        var vector2 = Vector3Struct.FromBytes(br.ReadBytes(12));

        return new BgfSkeletonStruct
        {
            Name = name,
            Vector1 = vector1,
            Unknown1 = unknown1,
            Vector2 = vector2
        };
    }
}
