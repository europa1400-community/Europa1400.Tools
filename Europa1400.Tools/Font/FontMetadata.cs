using System.Collections.Generic;
using Newtonsoft.Json;

namespace Europa1400.Tools.Font
{
    public class FontMetadata
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("atlasWidth")]
        public int AtlasWidth { get; set; }

        [JsonProperty("atlasHeight")]
        public int AtlasHeight { get; set; }

        [JsonProperty("lineHeight")]
        public int LineHeight { get; set; }

        [JsonProperty("baseLine")]
        public int BaseLine { get; set; }

        [JsonProperty("glyphs")]
        public List<FontGlyphInfo> Glyphs { get; set; } = new List<FontGlyphInfo>();
    }

    public class FontGlyphInfo
    {
        [JsonProperty("charCode")]
        public int CharCode { get; set; }

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("advance")]
        public int Advance { get; set; }

        [JsonProperty("offsetX")]
        public int OffsetX { get; set; }

        [JsonProperty("offsetY")]
        public int OffsetY { get; set; }
    }
}
