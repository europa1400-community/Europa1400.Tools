using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Bgf;

public class BgfMappingObjectStruct
{
    public required byte Unknown1 { get; init; }
    public required ushort Unknown2 { get; init; }
    public required ushort Unknown3 { get; init; }
    public required uint TextureCount { get; init; }
    public required uint VertexMappingCount { get; init; }
    public required uint PolygonMappingCount { get; init; }
    public required BgfVertexMapping[] VertexMappings { get; init; }
    public required BgfVertexMapping[] BoxVertexMappings { get; init; }
    public required float Unknown4 { get; init; }
    public required BgfPolygonMappingStruct[] PolygonMappings { get; init; }

    public static BgfMappingObjectStruct FromBytes(BinaryReader br)
    {
        br.SkipRequiredBytes(0x2F, 0x2D);
        var unknown1 = br.ReadByte();
        var unknown2 = br.ReadUInt16();
        br.Skip(1);
        var unknown3 = br.ReadUInt16();
        br.SkipRequiredBytes(0xB5, 0xFA);
        var textureCount = br.ReadUInt32();
        var vertexMappingCount = br.ReadUInt32();
        var polygonMappingCount = br.ReadUInt32();
        var vertexMappings = br.ReadArray(BgfVertexMapping.FromBytes, vertexMappingCount);
        var boxVertexMappings = br.ReadArray(BgfVertexMapping.FromBytes, 8);
        var unknown4 = br.ReadSingle();
        var polygonMappings = br.ReadArray(BgfPolygonMappingStruct.FromBytes, polygonMappingCount);

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