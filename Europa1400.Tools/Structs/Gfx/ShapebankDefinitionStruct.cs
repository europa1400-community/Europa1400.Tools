using System;
using System.IO;
using Europa1400.Tools.Extensions;

namespace Europa1400.Tools.Structs.Gfx
{
    public class ShapebankDefinitionStruct
    {
        public string Name { get; private set; }

        /// <summary>
        ///     Either base address of main shapebank or offset from base address of the main shapebank inc ase of child shapebank
        ///     definition
        /// </summary>
        public uint Address { get; private set; }

        public uint Size { get; private set; }
        public int ChildShapebankCount { get; private set; }
        public uint Unknown2 { get; private set; }
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }
        public bool IsFont => Name.StartsWith("_FONT");

        /// <summary>
        ///     Null when shapebank definition is main shapebank
        /// </summary>
        public ShapebankStruct? Shapebank { get; private set; }


        public static ShapebankDefinitionStruct FromBytes(BinaryReader br, bool shouldBeChildShapebank = false)
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

            if (!shouldBeChildShapebank)
            {
                var childDefs = br.ReadUntilException(r => FromBytes(r, true), typeof(FormatException));

                def.ChildShapebankCount = childDefs.Length;

                def.Shapebank = ShapebankStruct.FromBytes(br, address);
            }

            return def;
        }
    }
}