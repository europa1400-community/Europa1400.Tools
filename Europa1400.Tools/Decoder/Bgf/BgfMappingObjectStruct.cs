using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Bgf;

internal class BgfMappingObjectStruct
{
    internal required byte Unknown1 { get; init; }
    internal required ushort Unknown2 { get; init; }
    internal required ushort Unknown3 { get; init; }
    internal required uint TextureCount { get; init; }
    internal required uint VertexMappingCount { get; init; }
    internal required uint PolygonMappingCount { get; init; }
    internal required IEnumerable<BgfVertexMapping> VertexMappings { get; init; }
    internal required IEnumerable<BgfVertexMapping> BoxVertexMappings { get; init; }
    internal required float Unknown4 { get; init; }
    internal required IEnumerable<BgfPolygonMappingStruct> PolygonMappings { get; init; }

    internal static BgfMappingObjectStruct FromBytes(BinaryReader br)
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
