namespace Europa1400.Tools.Decoder.Structs;

public class AobjStruct
{
    public required IEnumerable<AobjObjectStruct> Objects { get; init; }

    public static AobjStruct FromBytes(byte[] data)
    {
        var objects = new List<AobjObjectStruct>();
        var offset = 0;

        while (offset < data.Length)
        {
            var obj = AobjObjectStruct.FromBytes(data[offset..]);
            objects.Add(obj);
            offset += 64;
        }

        return new AobjStruct
        {
            Objects = objects
        };
    }
}
