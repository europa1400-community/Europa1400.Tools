# Europa1400.Tools (C# / .NET)

.NET class library for decoding and converting **Europa 1400: The Guild / Die Gilde** (2002) game assets. Published as a NuGet package. Not an executable — consumers build their own CLI/GUI.

## Pipeline Architecture

```
GameAsset (on disk) → [Discoverer] → [Decoder] → Struct Object → [Converter] → IFileExport → [OutputHandler] → Disk
```

Fluent API: `PipelineBuilder<TAsset>.Create().DecodeWith<TDecoder>().ConvertWith<TConverter>().Write(options).Build()`

### Assembly

- **Target**: netstandard2.1
- **Nullable**: enabled
- **NuGet**: `Microsoft.CSharp`, `MP3Sharp`, `NAudio.Core`, `Newtonsoft.Json`, `SkiaSharp`

### Key Interfaces

| Interface | Role |
|-----------|------|
| `IAssetDiscoverer<TAsset>` | Scans game installation for asset files |
| `IDecoder` | Deserializes binary → struct objects |
| `ISubAssetProvider` | For decoders that yield multiple sub-assets (e.g., GFX → multiple shapes) |
| `IConverter` | Converts decoded structs → `IFileExport` objects |
| `IOutputHandler` | Writes exports to disk (JSON, PNG, WAV, raw copy) |

### Supported Formats

| Asset | File | Decoder | Converter | Output |
|-------|------|---------|-----------|--------|
| Ageb | `Data/A_Geb.dat` | `AgebDecoder` | — | JSON (88 buildings) |
| Aobj | `Data/A_Obj.dat` | `AobjDecoder` | — | JSON (732 objects) |
| Sbf | `sfx/*.sbf` | `SbfDecoder` | `SbfConverter` | WAV |
| Bgf | `Resources/objects.bin` → `*.bgf` | `BgfDecoder` | — | — |
| Gfx | `gfx/*.gfx` | `GfxDecoder` | `GfxConverter` | PNG (sprites) or atlas+JSON (fonts) |

### Structs (Binary Format Definitions)

- **`AgebStruct`** / **`AgebBuildingStruct`**: 88 buildings with group, name, level, time, price, coords
- **`AobjStruct`** / **`AobjObjectStruct`**: 732 objects with type, name, level, time, price
- **`SbfStruct`** / **`SoundbankStruct`**: WAV/MP3 sound containers
- **`BgfStruct`**: 3D models — header, textures, game objects + models (vertices/polygons), skeleton, mapping, footer
- **`GfxStruct`**: Shapebank definitions, `ShapebankStruct` ("SHAPBANK"), `GraphicStruct` with raw/RLE pixel data

All structs use `BinaryReaderExtensions` for Latin1 strings, marker-byte parsing, and generic array reading.

### Key Patterns

- **Pipeline Builder**: Fluent composition pattern with generics
- **Discoverer Self-Registration**: Static constructor registers each discoverer in `AssetDiscovererRegistry`
- **Marker-Byte Parsing**: Optional `0x28`/`0x37` byte sequences in BGF for conditional reading
- **JSON Cache**: Decoded assets cached as JSON in `./cache/decoded/`
- **Latin1 Encoding**: All string reading via `BinaryReaderExtensions.ReadString()` / `ReadCString()` / `ReadPaddedString()`

### SBF Sound Pipeline — WAV Header Fix

The game's SBF archive stores audio with `SoundDefinitionStruct.Length` describing the byte count to read. For `SoundType.Wav`, the raw bytes from the archive may contain RIFF headers with `ChunkSize` values that don't match the actual exported file size (due to alignment padding or imprecise length fields). **Unity depends on correct RIFF headers** to compute `AudioClip.length` — when the header says one size but the actual data is different, playback duration is wrong.

**Fix**: `SbfConverter` now re-encodes ALL audio through NAudio's `WaveFileReader` + `WaveFileWriter`. This parses the embedded WAV, extracts just the PCM data, and writes a fresh WAV with correctly calculated RIFF headers. Falls back to pass-through if NAudio cannot parse the data.

### Unimplemented / Stubs

- BGF has no converter (only decoder)
- BAF (animation) and TXS (texture mapping) assets declared but not decoded
- No executable entry point (`Program.cs`)

### Font Atlas Pipeline

**Problem**: Original game font glyphs are stored in GFX shapebanks (named `_FONT_*`). TextMeshPro requires a packed atlas + glyph metadata.

**Solution** (`Europa1400.Tools.Font` and `GfxConverter`):

When `GfxConverter` detects `ShapebankDefinitionStruct.IsFont` (`Name.StartsWith("_FONT")`), it outputs:

| Output | Path | Description |
|--------|------|-------------|
| Atlas PNG | `fonts/{name}/{name}.png` | All glyphs packed into power-of-two atlas |
| JSON metadata | `fonts/{name}/{name}.json` | Glyph rects, char codes, metrics |

**`Font/GlyphPacker.cs`** — Shelf-based rectangle packer (first-fit decreasing height):
- Sorts glyphs by height descending, places on horizontal shelves
- Power-of-two atlas sizing (min 64, max 4096)
- Configurable padding between glyphs

**`Font/FontMetadata.cs`** — JSON-serializable models:

```json
{
  "name": "_FONT_WHITE_9",
  "atlasWidth": 256,
  "atlasHeight": 128,
  "lineHeight": 11,
  "baseLine": 9,
  "glyphs": [
    { "charCode": 32, "x": 2, "y": 2, "width": 3, "height": 1,
      "advance": 4, "offsetX": 0, "offsetY": -8 }
  ]
}
```

**Character mapping**: Default assumes glyph index 0 → char code 32 (space), sequential. Override by modifying `CharCode` values in the output JSON.

**Metrics heuristics**:
- `lineHeight` = tallest glyph + 2px
- `baseLine` = tallest glyph (from top of line)
- `advance` = glyph width + 1
- `offsetY` = baseLine - glyph height (distance from baseline to glyph top)

**Unity integration** (not in this repo): Write a runtime script that reads the atlas PNG + JSON metadata, then creates a `TMP_FontAsset` programmatically via `FontEngine` APIs.

### Build & Test

```powershell
dotnet restore && dotnet build
dotnet test    # requires .env with GAME_DIRECTORY_PATH
```

### Output Handlers

| Handler | Usage |
|---------|-------|
| `ObjectSerializationOutputHandler` | Decode-only: struct → JSON |
| `FileExportOutputHandler` | Converted: `IFileExport` (PNG/WAV) to disk |
| `RawFileCopyOutputHandler` | No decoder: copy raw files |
