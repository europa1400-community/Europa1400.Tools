using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Aobj;

internal class AobjStruct
{
    internal required AobjObjectStruct[] Objects { get; init; }

    internal static AobjStruct FromBytes(BinaryReader br)
    {
        var objects = br.ReadArray(AobjObjectStruct.FromBytes, 732);

        return new AobjStruct
        {
            Objects = objects
        };
    }
}
