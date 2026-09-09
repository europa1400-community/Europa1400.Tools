using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Europa1400.Tools.Structs.Bgf
{
    internal readonly struct BgfVisualFace
    {
        public uint A { get; init; }
        public uint B { get; init; }
        public uint C { get; init; }
        public int TextureIndex { get; init; }
        public Vector2 Uv1 { get; init; }
        public Vector2 Uv2 { get; init; }
        public Vector2 Uv3 { get; init; }
    }

    internal static class BgfVisualGeometry
    {
        /// <summary>
        /// Use the runtime faces with the runtime vertices. Their material indices address
        /// the footer texture table, and their UVs already use the top-left image origin
        /// required by glTF. The tagged GameObject models use editor UVs (V upwards).
        /// </summary>
        internal static IReadOnlyList<BgfVisualFace> GetFaces(BgfStruct bgf)
        {
            return bgf.MappingObject.PolygonMappings.Select(p => new BgfVisualFace
            {
                A = p.Face.A,
                B = p.Face.B,
                C = p.Face.C,
                TextureIndex = bgf.ResolveFileTextureIndex(p.TextureIndex),
                Uv1 = ToGltfUv(p.TextureMapping.TextureCoordinates1),
                Uv2 = ToGltfUv(p.TextureMapping.TextureCoordinates2),
                Uv3 = ToGltfUv(p.TextureMapping.TextureCoordinates3)
            }).ToArray();
        }

        private static Vector2 ToGltfUv(BgfTextureCoordinatesStruct uv)
        {
            // The runtime adds one U repeat relative to the editor data. Keep fractional
            // and out-of-range coordinates intact: wrapping happens after interpolation.
            return new Vector2(uv.U - 1f, uv.V);
        }
    }
}
