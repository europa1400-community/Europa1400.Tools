using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Decoder.Gfx;

internal class ShapebankDefinitionStruct
{
    internal required string Name { get; init; }
    
    /// <summary>
    /// Either base address of main shapebank or offset from base address of the main shapebank inc ase of child shapebank definition
    /// </summary>
    internal required uint Address { get; init; }
    internal required uint Size { get; init; }
    internal int ChildShapebankCount { get; private set; }
    internal required uint Unknown2 { get; init; }
    internal required ushort Width { get; init; }
    internal required ushort Height { get; init; }
    internal bool IsFont => Name.StartsWith("_FONT");
    
    /// <summary>
    /// Null when shapebank definition is main shapebank
    /// </summary>
    internal ShapebankStruct? Shapebank { get; private set; }


    internal static ShapebankDefinitionStruct FromBytes(BinaryReader br, bool shouldBeChildShapebank = false)
    {
        var pos = br.BaseStream.Position;
        
        var name = br.ReadPaddedString(48);
        var address = br.ReadUInt32();
        br.Skip(4);
        var size = br.ReadUInt32();
        var childShapebankCount = br.ReadUInt32();
        br.Skip(4);
        var magic2 = br.ReadByte();
        br.Skip(7);
        var magic3 = br.ReadUInt32();
        var width = br.ReadUInt16();
        var height = br.ReadUInt16();
        
        var def = new ShapebankDefinitionStruct
        {
            Name = name,
            Address = address,
            Size = size,
            Unknown2 = magic2,
            Width = width,
            Height = height
        };

        if (shouldBeChildShapebank && address != 0)
        {
            br.BaseStream.Position = pos;
            throw new FormatException("Shapebank is new main shapebank");   
        }
        
        if(!shouldBeChildShapebank)
        {
            var childDefs = br.ReadUntilException(r => FromBytes(r, true), typeof(FormatException));

            def.ChildShapebankCount = childDefs.Length;
            
            def.Shapebank = ShapebankStruct.FromBytes(br, address);
        }

        return def;
    }
}
