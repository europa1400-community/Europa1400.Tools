# BGF texture mapping

The visual export uses the runtime MappingObject for positions, face indices and UVs.
Do not combine it with tagged GameObject/editor polygons based on matching counts.
In the Gold Edition corpus, 1,083 of 3,023 objects have matching counts; the editor and
runtime UVs still differ for parts of `_Addon/Friedhof/ob_GRAEBER_MITTEL.bgf`.

## Runtime texture table

Exactly `MappingObject.TextureCount` records follow the runtime polygons:

| Field | Encoding |
| --- | --- |
| Name | Latin1, null terminated |
| SecondaryName | Latin1, null terminated; can be present when Name is empty |
| NameAppendix | Latin1, null terminated |
| Metadata | 6 uninterpreted bytes |

Use Name, or SecondaryName when Name is empty, to resolve the corresponding file
texture. A face's TextureIndex addresses this runtime table, not the tagged texture
list at the start of the file. For example, character head/body entries can have
different orders in the two tables. Index 255 is not a texture-table entry.

All three strings must be read even when empty. Scanning for printable bytes is
incorrect: metadata can contain printable characters (including 0xCB, 0x7E and 0x2F).
Examples exercising the layout include `ob_PIPETTE_PARFUM.bgf`,
`ub_SEKTGLAS.bgf` (secondary name) and `ob_EXEKUTIONSKANONE.bgf`
(appendix "Kiste"). Attachment data follows the counted texture table and must not
be consumed as texture names. The remaining metadata semantics and attachment
decoding are not changed by this fix.

## UV conventions

Runtime polygons store three U values, three V values, then a face normal (the
existing model exposes that third vector as ValuesW). UV coordinates remain
attached to the corresponding A/B/C corner, including at seams.

Runtime V already runs downwards. glTF and the WPF preview both use the top-left
image origin; the preview must not flip GLB V again. OBJ uses the opposite V
convention, so that conversion applies `1 - V` when writing `vt`.

The runtime normally adds one U repeat relative to editor UVs; export removes that
integer offset. Preserve out-of-range UVs for tiling; do not clamp or apply modulo
per vertex, since interpolation across tile boundaries would change.

glTF convention: [Khronos glTF 2.0 specification, Images](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#images).

## Verification

`BgfTextureMappingTests` covers binary metadata, optional names, attachment boundaries,
invalid indices, conflicting editor/runtime data, and actual GLB/OBJ exports with
asymmetric and out-of-range UVs. `BgfRealFileTests` reads objects.bin from the local
installation and checks that every textured runtime face resolves to a declared
texture. All 3,023 Gold Edition models passed that check.

Preview comparisons used Arbeiter_MANN, ob_SPIELERTAFEL_WH and gb_LAGERHALLE.
Restart/rebuild the Verifier to load the updated assemblies; its assembly-based
cache stamp invalidates previously converted previews.
