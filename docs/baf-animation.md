# BAF animation binding and GLB playback

Verified against the installed Gold Edition archives on 2026-09-09. The implementation is
in `Structs/Baf/BafStruct.cs`, `Animation/BgfAnimationCatalog.cs`,
`Pipeline/Converter/BgfAnimationConverter.cs`, and the Verifier.

## What a BAF animates

BAFs contain sampled **mesh vertex positions**, not a weighted bone skeleton. A frame
contains one vertex block per original model object. Those blocks are concatenated in
file order and address the BGF runtime vertex array in the same order.

The exported pose is:

```text
exported_position[v,t] = BGF_rest[v] + (BAF_position[v,t] - BAF_position[v,0])
```

This preserves the selected BGF's proportions and texture assignments. Replacing its
vertices with the absolute BAF positions would give every character variant the
animation author's reference shape.

The runtime BGF faces, UVs, and material indices remain the source of the visual mesh;
see [BGF texture mapping](bgf-texture-mapping.md). Runtime coordinates are converted
to glTF as `(X, Y, -Z)`. The Verifier camera also uses Y as up.

## Binary layout

All integers and floats below are little endian. Strings are null-terminated Latin1.
The file signature is `BGF\0`, even though the extension is BAF.

| Marker | Payload | Meaning |
| --- | --- | --- |
| `30` | uint32 | Data size; observed as file length minus 10, not a seek target |
| `01` | uint32 `ABCD0001` | Version |
| `23` | uint32 | Number of frames |
| `33` | byte | Vertex animation flag; omitted in some old files |
| `24` | byte | Additional animation flag |
| `37` | uint32 | Attachment count |
| `36` | uint32 | Object blocks per frame; defaults to one |
| `34` | uint32 | Total vertices; infer from first frame when absent |
| `29`, `2A` | int32 each | Loop-in and loop-out **frame array indices** |
| `18` | uint32 | Key time |
| `19` | uint32 | Vertex count for this object |
| `21` | count × float3 | Positions, followed by `28` |
| `31` | float3 + float3 | Optional motion position and rotation after all object blocks |
| `38` | string | Attachment name |
| `39` | float3 | Attachment position |
| `3A` | float3 | Attachment rotation |
| `2F` | — | End of file |

The decoder reads `frameCount × objectCount` object blocks, but returns only
`frameCount` frames. It checks matching object times, stable vertex layouts, increasing
key times, declared totals, and truncated blocks.

For example, `faellen_BUCHE_01.baf` has **5 frames and 16 objects**, with layout
`[4,4,4,4,4,4,4,4,4,4,4,4,4,4,30,5]`. It is not an 80-frame animation.
`gehen_MANN.baf` has 68 frames, 323 vertices, and three attachment tracks.

`BoneCount` remains as a legacy public property for source compatibility;
`VertexCount` is its correctly named alias. Attachment records such as
`d3_Head`, `d3_LeftHand`, and `d3_RightHand` are attachment anchors, not skin weights.

## Evidence from GildeGold.exe

Inspected statically; no modified EXE or runtime patch is required.

SHA256:
`D6F20FDA136CD7CED612B96094E527A137F129543753F0FEC0E357F6AECB3074`

Addresses are virtual addresses for this specific executable:

- `0x44F8C0`: BAF loader, including frame/object counts and optional markers.
- `0x44FD39–0x44FDA6`: subtracts frame zero's positions from subsequent frames.
- `0x4500A2–0x4500BE`: adjacent stored key values are subtracted to obtain interval lengths.
- `0x450104–0x4501AC`: motion values are also converted relative to frame zero.
- `0x45027A–0x4502FF`: depending on the loader's caller-supplied movement mode,
  subtracts motion from vertex deformation.
- `0x531C01–0x531C05`: starts the timer with a **14 ms** period.
- `0x42E8C0` / `0x42E912`: sets up the Windows `timeSetEvent` timer.
- `0x42E870–0x42E876`: increments global game tick `0x6333CC`.
- `0x43E51B–0x43E542`: passes that tick to the object animation update callback.
- `0x4355DA`: initializes playback speed to 1.
- `0x432969–0x4329A9`: advances animation keys using elapsed ticks times playback speed,
  retaining fractional progress.

The default export time is therefore `(key - firstKey) * 0.014 seconds`.
For `gehen_MANN`, keys 0 through 105 give **1.47 seconds** at the default engine speed.
These are nominal game ticks, not a 25- or 30-fps video timebase. Game/script speed
overrides are not inferred from an isolated BAF. The converter accepts
`keyTicksPerSecond`, and preview speed can be adjusted independently.

The sibling INIs contain authoring/export settings; their `NumKeys` can disagree with
the binary samples. Conversion uses the binary key list, not an INI-generated frame list.

## Binding models to clips

Automatic selection requires an exact **ordered per-object vertex layout**, as well as:

- For characters: a matching family token (`MANN`, `MANN2`, `FRAU`, etc.) and matching
  high/low detail. The BGF directory `Low_Poly_Character` corresponds to BAF
  `LowPolyCharacter`.
- For other objects: a shared descriptive name token and matching explicit numeric
  variants when both names contain them. Thus `BUCHE_01` does not receive `BUCHE_02`.

This is a conservative candidate search from archive naming and topology, not a
complete reimplementation of the game's script/OGR animation dispatch. Equal vertex
counts alone do not prove semantic compatibility. The public converter also accepts
an explicitly selected BAF, provided the ordered layouts agree.

## GLB representation

Each selected clip produces its own GLB containing a standard glTF `weights` animation.
Every source frame becomes a morph target with position and recomputed normal deltas.
Weights interpolate linearly at the **original, potentially nonuniform key times**.

Source vertex identity is kept when expanding triangle corners. Vertices that coincide
in the rest pose must not be welded if their animation differs. The animated mesh is
built directly, preserving triangles that may collapse at rest. Base normals are
normalized, with a fallback for zero/invalid normals. If a texture cannot be resolved,
the animated geometry gets a neutral material with `extras.missingTexture` naming the
missing source texture.

Horizontal motion from marker `31` is subtracted from the morph positions and exported
as a node translation channel. Their sum reproduces the full sampled pose. Vertical
motion and rotation remain baked into the vertex deformation. The node is tagged
`extras.bgfRootMotion`, allowing the Verifier's **In place** option to suppress just
that translation. This option changes the preview, not the GLB file.

Animation extras preserve `loopStart`, `loopEnd`, and `sourceKeyTicksPerSecond`.
Standard glTF players can play the clip without a custom extension; the extras describe
the optional loop interval and preview behavior.

One clip per file bounds memory and file size for models with hundreds of possible
animations. For example, the worker has 149 matching clips; selecting one converts only
that clip.

## Using the Verifier

1. Start `dotnet run --project Europa1400.Tools.Verifier -c Release`.
2. Choose **Models**, then e.g. `_DYNAMIC/Character/Arbeiter_MANN.bgf`.
3. Keep **glTF (GLB)** selected and choose `Character/MANN/Bewegung/gehen_MANN.baf`
   in **Animation**.
4. Use **Play/Pause**, **Stop**, the timeline, **Loop**, **In place**, and **Speed**.
5. Select **Rest pose** to return to the static GLB.

The export is stored under `cache/verifier/BgfAsset/glb-animation-.../`.
The animation cache key includes the BAF path, file size, and modification time;
the existing cache also tracks the BGF and library build. **Reconvert** refreshes the
currently selected export. A running older Verifier must be restarted to use new code.

The WPF preview reads and samples the animation embedded in the exported GLB; it does
not deform the preview directly from the BAF. Rapid selection changes cancel pending
work, and loading another model stops playback.

## Library example

```csharp
using var reader = new BinaryReader(File.OpenRead(bgfPath));
var bgf = BgfStruct.FromBytes(reader);
var catalog = BgfAnimationCatalog.FromGameInstallation(gamePath, cacheRoot);
var clip = catalog.FindForModel(bgf, bgfPath).First(c => c.Name == "gehen_MANN");
var textures = BgfConverter.EnsureTextures(gamePath, Path.Combine(cacheRoot, "textures"));
var glb = BgfAnimationConverter.Convert(bgf, clip.Decode(), clip.Name, textures, txs: null);
glb.SaveGLB("Arbeiter_MANN--gehen_MANN.glb");
```

Pass the sibling TXS when the model relies on a texture override.

## Validation and remaining scope

- All **1,251 BAF files** in the installed archive decode completely with no trailing data.
- **1,119 distinct automatically matched clips**, across 63 representative BGFs, were
  converted, written, and loaded again with SharpGLTF validation: **zero failures**.
  This does not assert that all 1,251 clips have an automatic model association or that
  every visual combination has been reviewed.
- Synthetic regressions cover multipart frames, optional headers, truncation, relative
  deformation, nonuniform timing, coincident vertices, collapsed faces, invalid normals,
  missing textures, and family/detail/variant mismatches.
- WPF tests sample the actual GLB positions/normals/UVs and exercise the real worker
  selection/export/play/pause/seek/stop/rest-pose UI path.
- Worker and multipart tree animation frames were rendered for visual inspection.

Attachment tracks are decoded, but separate hand/head props and attachment rotations
are not yet exported as animated nodes. The game also has blending, movement modes,
and scripted actions beyond a standalone clip. The exporter uses linear interpolation
between stored poses and does not claim to reproduce all runtime interpolation or
transition behavior. Existing foliage alpha/material limitations are separate from
the vertex animation mapping.
