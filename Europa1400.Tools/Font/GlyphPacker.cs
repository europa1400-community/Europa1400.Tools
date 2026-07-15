using System;
using System.Collections.Generic;
using System.Linq;

namespace Europa1400.Tools.Font
{
    public class GlyphPacker
    {
        public class PackedGlyph
        {
            public int Index { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        public static (PackedGlyph[] Packed, int AtlasWidth, int AtlasHeight) Pack(
            int[] widths,
            int[] heights,
            int padding = 1)
        {
            var count = widths.Length;
            var indices = Enumerable.Range(0, count)
                .OrderByDescending(i => heights[i])
                .ThenByDescending(i => widths[i])
                .ToArray();

            var totalArea = 0;
            for (var i = 0; i < count; i++)
                totalArea += (widths[i] + padding) * (heights[i] + padding);

            var atlasWidth = NextPowerOfTwo((int)Math.Ceiling(Math.Sqrt(totalArea)));
            if (atlasWidth < 64) atlasWidth = 64;
            if (atlasWidth > 4096) atlasWidth = 4096;

            var shelves = new List<(int y, int height, int usedWidth)>();
            var result = new PackedGlyph[count];

            foreach (var idx in indices)
            {
                var gw = widths[idx] + padding * 2;
                var gh = heights[idx] + padding * 2;
                var placed = false;

                for (var s = 0; s < shelves.Count; s++)
                {
                    var shelf = shelves[s];
                    if (shelf.usedWidth + gw <= atlasWidth && shelf.height >= gh)
                    {
                        result[idx] = new PackedGlyph
                        {
                            Index = idx,
                            X = shelf.usedWidth + padding,
                            Y = shelf.y + padding,
                            Width = widths[idx],
                            Height = heights[idx]
                        };
                        shelves[s] = (shelf.y, shelf.height, shelf.usedWidth + gw);
                        placed = true;
                        break;
                    }
                }

                if (placed) continue;

                var newY = shelves.Count > 0
                    ? shelves.Last().y + shelves.Last().height
                    : 0;

                shelves.Add((newY, gh, gw));
                result[idx] = new PackedGlyph
                {
                    Index = idx,
                    X = padding,
                    Y = newY + padding,
                    Width = widths[idx],
                    Height = heights[idx]
                };
            }

            var atlasHeight = shelves.Count > 0
                ? shelves.Last().y + shelves.Last().height
                : 0;

            atlasHeight = NextPowerOfTwo(Math.Max(atlasHeight, 64));
            if (atlasHeight > 4096) atlasHeight = 4096;
            if (atlasWidth > 4096) atlasWidth = 4096;

            return (result, atlasWidth, atlasHeight);
        }

        private static int NextPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }
    }
}
