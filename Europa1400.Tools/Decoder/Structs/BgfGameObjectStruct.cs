using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Structs;

public class BgfGameObjectStruct
{
    public required string Name { get; init; }
    public BgfModelStruct? Model { get; init; }
    public int? SkeletonCount { get; init; }
    public IEnumerable<BgfSkeletonStruct>? Skeletons { get; init; }

    public static BgfGameObjectStruct FromBytes(BinaryReader br)
    {
        br.SkipOptionalByte(0x28);
        br.SkipRequiredBytes(0x14, 0x15);
        var name = br.ReadCString();
        var wasSkipped1 = br.SkipOptionalBytesAll(0x16, 0x01);
        if (wasSkipped1) br.Skip(3);
        var wasSkipped2 = br.SkipOptionalBytesAll(0x17, 0x18);
        if (wasSkipped2) br.Skip(4);
        var model = wasSkipped2 ? BgfModelStruct.FromBytes(br) : null;
        br.SkipOptionalByte(0x28);
        br.SkipOptionalByte(0x28);
        br.SkipOptionalByte(0x28);
        var wasSkipped3 = br.SkipOptionalBytesAll(0x37);
        var skeletonCount = wasSkipped3 ? br.ReadInt32() as int? : null;
        var skeletons = wasSkipped3 ? Enumerable.Range(0, skeletonCount ?? 0).Select(_ => BgfSkeletonStruct.FromBytes(br)) : null;

        return new BgfGameObjectStruct
        {
            Name = name,
            Model = model,
            SkeletonCount = skeletonCount,
            Skeletons = skeletons
        };
    }
}
