namespace Europa1400.Tools.Gltf;

internal class GltfPrimitiveData
{
    internal string TextureName { get; init; }
    internal string TexturePath { get; init; }
    internal bool IsTransparentTexture { get; init; }
    internal GltfTriangleData[] Triangles { get; init; }
}