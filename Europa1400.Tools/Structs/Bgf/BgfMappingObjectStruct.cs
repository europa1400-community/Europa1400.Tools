using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Bgf
{
    public class BgfMappingObjectStruct
    {
        public byte Unknown1 { get; private set; }
        public ushort Unknown2 { get; private set; }
        public ushort Unknown3 { get; private set; }
        public uint TextureCount { get; private set; }
        public uint VertexMappingCount { get; private set; }
        public uint PolygonMappingCount { get; private set; }
        public BgfVertexMapping[] VertexMappings { get; private set; }
        public BgfVertexMapping[] BoxVertexMappings { get; private set; }
        public float Unknown4 { get; private set; }
        public BgfPolygonMappingStruct[] PolygonMappings { get; private set; }

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
}