# Europa1400.Tools

.NET library for decoding and converting **Europa 1400: The Guild / Die Gilde** (2002) game assets. Published as a NuGet package.

Target: `net10.0`, nullable enabled.

## Pipeline

Everything goes through one path:

```text
GameAsset (on disk)
    → Discoverer
    → Decoder          → struct
    → Converter        → IFileExport   (optional)
    → OutputHandler    → disk
```

Fluent API:

```csharp
await PipelineBuilder<SbfAsset>
    .Create()
    .DecodeWith<SbfDecoder>()
    .ConvertWith<SbfConverter>()
    .Write(new OutputHandlerOptions { OutputRoot = "output/sounds" })
    .Build()
    .ExecuteAsync(GameAssets.OfType<SbfAsset>().FromGameInstallation(gamePath));
```

Decode-only (struct serialized as JSON, no converter):

```csharp
await PipelineBuilder<AgebAsset>
    .Create()
    .DecodeWith<AgebDecoder>()
    .Write(new OutputHandlerOptions { OutputRoot = "output" })
    .Build()
    .ExecuteAsync(GameAssets.OfType<AgebAsset>().FromGameInstallation(gamePath));
```

`Write()` picks the handler from the object type: `IFileExport` → bytes on disk, decoded struct → JSON, `GameAsset` → raw copy.

## Layout

| Path | Role |
| ------ | ------ |
| `Pipeline/Assets/` | `GameAsset` subclasses (`SbfAsset`, `GfxAsset`, …) |
| `Pipeline/Discoverer/` | Find files in a game install |
| `Pipeline/Decoder/` | Binary file → struct |
| `Pipeline/Converter/` | Struct → `IFileExport` (PNG, WAV, GLB, JSON, …) |
| `Pipeline/Output/` | Write exports to disk |
| `Structs/` | Binary format definitions (`FromBytes`) |
| `Extensions/BinaryReaderExtensions.cs` | Latin1 strings, marker bytes, arrays |

## Adding a format

1. **Struct** in `Structs/<Format>/` with a public `FromBytes(BinaryReader)` factory. Use `BinaryReaderExtensions` (`ReadCString`, `ReadPaddedString`, `ReadArray`, `SkipRequiredByte`, …). Game strings are Latin1, not UTF-8.

2. **Asset** in `Pipeline/Assets/` — empty subclass of `GameAsset` is enough.

3. **Discoverer** — glob formats use `GlobAssetDiscoverer<T>` in `AssetDiscovererRegistry`. Custom layouts (archives, sibling `.ini` files) get their own class. Register it in the registry static constructor.

4. **Decoder** in `Pipeline/Decoder/` implementing `IDecoder`. Binary files can go through `BinaryAssetDecoder.Decode(asset, Struct.FromBytes)`.

5. **Converter** only if the output is not “the struct as JSON”. Sprites, audio, meshes: implement `IConverter` and return `IFileExport` (`FilePath` + `Content`). JSON dumps of the decoded object are already handled by `Write()` without a converter.

6. **Tests** in `Europa1400.Tools.Tests` — synthetic bytes for the decoder, plus a pipeline run against the real game install when it makes sense.

## Conventions

- One discoverer / decoder / converter per format. Shared glob search lives in `GlobAssetDiscoverer<T>`, shared binary open+read in `BinaryAssetDecoder`, shared JSON/byte exports in `FileExportFactory`.
- Decoded structs are the public model of a format. Converters should not invent a parallel DTO unless the export shape is genuinely different (e.g. a font atlas).
- GFX shapebanks named `_FONT_*` go through `GfxConverter` as an atlas PNG + glyph JSON (`Font/`).
- Decoded assets are cached as JSON under `./cache/decoded/` so convert/write can rerun without re-parsing.

## Formats

| Asset | Source | Decoder | Converter | Typical output |
| ------- | -------- | --------- | ----------- | ---------------- |
| Ageb | `Data/A_Geb.dat` | `AgebDecoder` | — | JSON |
| Aobj | `Data/A_Obj.dat` | `AobjDecoder` | — | JSON |
| Sbf | `sfx/*.sbf` | `SbfDecoder` | `SbfConverter` | WAV |
| Gfx | `gfx/*.gfx` | `GfxDecoder` | `GfxConverter` | PNG / font atlas |
| Bgf | `Resources/objects.bin` → `*.bgf` | `BgfDecoder` | `BgfConverter` | GLB |
| Baf | `Resources/animations.bin` → `*.baf` | `BafDecoder` | `BgfAnimationConverter` (with a matching BGF) | Animated GLB / JSON |
| Ed3 / Ogr / Res / Sav / Scr / Txs | matching extension | matching decoder | matching converter where present | JSON / bytes |

`FormDecoder` is still a stub.

## Verifier

`Europa1400.Tools.Verifier` is a WPF app for checking discovery, decode, and convert against a real install. It lists assets by category, converts the selected file on demand, and previews the result (PNG, WAV, JSON, …).

Converted output is cached under `cache/verifier/` and reused until the `Europa1400.Tools` assembly or the source file changes. Archives (`objects.bin`, `animations.bin`) are extracted once and reused the same way. Use **Reconvert** to bypass the cache for the current asset.

```powershell
dotnet run --project Europa1400.Tools.Verifier
```

Uses the same `.env` `GAME_DIRECTORY_PATH` as the tests.

For animated models, select **Models → glTF (GLB)** and choose a clip in **Animation**.
The resulting GLB contains the animation; use Play/Pause, Stop, the timeline, Loop,
In place, and Speed to inspect it. Each clip is exported separately on demand.
See [BAF animation format, binding, and EXE findings](docs/baf-animation.md).

## Build & test

```powershell
dotnet restore && dotnet build
dotnet test
```

Tests that hit a real install need a `.env` at the solution root:

```ini
GAME_DIRECTORY_PATH=C:\path\to\Europa1400
```
