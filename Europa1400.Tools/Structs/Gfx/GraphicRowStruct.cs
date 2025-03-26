using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Gfx
{
    public class GraphicRowStruct
    {
        public uint BlockCount { get; set; }
        public TransparencyBlockStruct[] TransparencyBlocks { get; set; }

        public static GraphicRowStruct FromBytes(BinaryReader br)
        {
            var blockCount = br.ReadUInt32();
            var transparency = br.ReadArray(TransparencyBlockStruct.FromBytes, blockCount);

            return new GraphicRowStruct
            {
                BlockCount = blockCount,
                TransparencyBlocks = transparency
            };
        }
    }
}