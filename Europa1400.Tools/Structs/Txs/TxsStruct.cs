using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Txs;

public class TxsStruct
{
    public required uint Unknown1 { get; init; }
    public required uint Unknown2 { get; init; }
    public required uint Unknown3 { get; init; }
    public required string[] TextureNames { get; init; }

    public static TxsStruct FromBytes(BinaryReader br)
    {
        var unknown1 = br.ReadUInt32();
        var unknown2 = br.ReadUInt32();
        var unknown3 = br.ReadUInt32();
        var textureNames = br.ReadArray(reader => reader.ReadCString(), unknown2 * unknown3);
        
        return new TxsStruct
        {
            Unknown1 = unknown1,
            Unknown2 = unknown2,
            Unknown3 = unknown3,
            TextureNames = textureNames
        };
    }
}