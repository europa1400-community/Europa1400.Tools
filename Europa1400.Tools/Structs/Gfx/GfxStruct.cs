﻿using System.Collections.Generic;
using System.IO;

namespace Europa1400.Tools.Structs.Gfx
{
    public class GfxStruct
    {
        public uint ShapebankCount { get; set; }
        public ShapebankDefinitionStruct[] ShapebankDefinitions { get; set; }

        public static GfxStruct FromBytes(BinaryReader br)
        {
            var shapebankCount = br.ReadUInt32();

            var shapebankDefinitions = new List<ShapebankDefinitionStruct>();

            for (var i = 0; i < shapebankCount; i++)
            {
                var def = ShapebankDefinitionStruct.FromBytes(br);

                i += def.ChildShapebankCount;

                shapebankDefinitions.Add(def);
            }

            return new GfxStruct
            {
                ShapebankCount = shapebankCount,
                ShapebankDefinitions = shapebankDefinitions.ToArray()
            };
        }
    }
}