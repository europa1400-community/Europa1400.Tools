using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Structs;

public class BgfMappingObjectStruct
{
    public byte Unknown1 { get; init; }
    public short Unknown2 { get; init; }
    public short Unknown3 { get; init; }
    public int TextureCount { get; init; }
    public int VertexMappingCount { get; init; }
    public int PolygonMappingCount { get; init; }
    public required IEnumerable<BgfVertexMapping> VertexMappings { get; init; }
    public required IEnumerable<BgfVertexMapping> BoxVertexMappings { get; init; }
    public float Unknown4 { get; init; }
    public required IEnumerable<BgfPolygonMappingStruct> PolygonMappings { get; init; }

    public static BgfMappingObjectStruct FromBytes(BinaryReader br)
    {
        br.SkipRequiredBytes(0x2F, 0x2D);
        var unknown1 = br.ReadByte();
        var unknown2 = br.ReadInt16();
        br.Skip(1);
        var unknown3 = br.ReadInt16();
        br.SkipRequiredBytes(0xB5, 0xFA);
        var textureCount = br.ReadInt32();
        var vertexMappingCount = br.ReadInt32();
        var polygonMappingCount = br.ReadInt32();
        var vertexMappings = Enumerable.Range(0, vertexMappingCount).Select(_ => BgfVertexMapping.FromBytes(br));
        var boxVertexMappings = Enumerable.Range(0, 8).Select(_ => BgfVertexMapping.FromBytes(br));
        var unknown4 = br.ReadSingle();
        var polygonMappings = Enumerable.Range(0, polygonMappingCount).Select(_ => BgfPolygonMappingStruct.FromBytes(br));

        return new BgfMappingObjectStruct
        {
            Unknown1 = unknown1,
            Unknown2 = unknown2,
            Unknown3 = unknown3,
            TextureCount = textureCount,
            VertexMappingCount = vertexMappingCount,
            PolygonMappingCount = polygonMappingCount,
            VertexMappings = vertexMappings,
            BoxVertexMappings = boxVertexMappings,
            Unknown4 = unknown4,
            PolygonMappings = polygonMappings
        };
    }
}
