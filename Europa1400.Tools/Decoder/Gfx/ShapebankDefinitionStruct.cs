using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Gfx;

internal class ShapebankDefinitionStruct
{
    public required string Name { get; init; }

    public required uint Address { get; init; }

    public required byte[] Zeroes1 { get; init; }

    public required uint Size { get; init; }

    public required uint Magic1 { get; init; }

    public required byte[] Zeroes2 { get; init; }

    public required bool MagicFlag { get; init; }

    public required byte[] Zeroes3 { get; init; }

    public required uint Magic2 { get; init; }

    public required ushort Width { get; init; }

    public required ushort Height { get; init; }

    public bool IsMainShapebank => Address != 0;

    public bool IsFont => Width == 0 && Height == 0;

    public required ShapebankStruct? Shapebank { get; init; }


    public static ShapebankDefinitionStruct FromBytes(BinaryReader br)
    {
        var name = br.ReadPaddedString(48);
        var address = br.ReadUInt32();
        var zeroes1 = br.ReadBytes(4);
        var size = br.ReadUInt32();
        var magic1 = br.ReadUInt32();
        var zeroes2 = br.ReadBytes(4);
        var magicFlag = br.ReadByte() == 1;
        var zeroes3 = br.ReadBytes(7);
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
            Zeroes1 = zeroes1,
            Size = size,
            Magic1 = magic1,
            Zeroes2 = zeroes2,
            MagicFlag = magicFlag,
            Zeroes3 = zeroes3,
            Magic2 = magic2,
            Width = width,
            Height = height,
            Shapebank = shapebank
        };
    }
}
