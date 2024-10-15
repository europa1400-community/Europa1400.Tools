using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Gfx;

internal class ShapebankDefinitionStruct
{
    internal required string Name { get; init; }
    internal required uint Address { get; init; }
    internal required uint Size { get; init; }
    internal required uint Unknown1 { get; init; }
    internal required bool Unknown2 { get; init; }
    internal required uint Unknown3 { get; init; }
    internal required ushort Width { get; init; }
    internal required ushort Height { get; init; }
    internal bool IsMainShapebank => Address != 0;
    internal bool IsFont => Width == 0 && Height == 0;
    internal required ShapebankStruct? Shapebank { get; init; }


    internal static ShapebankDefinitionStruct FromBytes(BinaryReader br)
    {
        var name = br.ReadPaddedString(48);
        var address = br.ReadUInt32();
        br.Skip(4);
        var size = br.ReadUInt32();
        var magic1 = br.ReadUInt32();
        br.Skip(4);
        var magicFlag = br.ReadByte() == 1;
        br.Skip(7);
        var magic2 = br.ReadUInt32();
        var width = br.ReadUInt16();
        var height = br.ReadUInt16();

        ShapebankStruct? shapebank = null;

        if (address != 0)
        {
            var currentPosition = br.BaseStream.Position;
            
            br.BaseStream.Position = address;
            shapebank = ShapebankStruct.FromBytes(br);

            br.BaseStream.Position = currentPosition;
        }
        
        return new ShapebankDefinitionStruct
        {
            Name = name,
            Address = address,
            Size = size,
            Unknown1 = magic1,
            Unknown2 = magicFlag,
            Unknown3 = magic2,
            Width = width,
            Height = height,
            Shapebank = shapebank
        };
    }
}
