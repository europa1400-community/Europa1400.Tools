using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using SharpGLTF.Schema2;
using GltfMaterial = SharpGLTF.Schema2.Material;
using WpfMaterial = System.Windows.Media.Media3D.Material;
using MediaColor = System.Windows.Media.Color;

namespace Europa1400.Tools.Verifier;

internal static class ModelPreviewLoader
{
    public static Model3D Load(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".glb" or ".gltf" => LoadGltf(ModelRoot.Load(path)),
            ".obj" => LoadObj(path),
            _ => throw new NotSupportedException($"No 3D preview for {ext}.")
        };
    }

    private static Model3D LoadObj(string path)
    {
        var reader = new ObjReader();
        var model = reader.Read(path);
        if (model == null)
            throw new InvalidOperationException("OBJ reader returned no model.");
        return model;
    }

    public static GltfAnimationPreview LoadAnimated(string path)
    {
        var root = ModelRoot.Load(path);
        return new GltfAnimationPreview(root, LoadGltf(root));
    }

    private static Model3DGroup LoadGltf(ModelRoot root)
    {
        var group = new Model3DGroup();
        var materials = new Dictionary<int, WpfMaterial>();
        var scene = root.DefaultScene ?? root.LogicalScenes.FirstOrDefault()
            ?? throw new InvalidOperationException("GLB has no scene.");

        foreach (var node in scene.VisualChildren)
            AddNode(node, group, materials);

        if (group.Children.Count == 0)
            throw new InvalidOperationException("GLB has no renderable mesh (often missing textures).");



        return group;
    }

    private static void AddNode(Node node, Model3DGroup group, Dictionary<int, WpfMaterial> materials)
    {
        if (node.Mesh != null)
        {
            var world = node.WorldMatrix;
            foreach (var primitive in node.Mesh.Primitives)
            {
                var model = CreatePrimitive(primitive, world, materials);
                if (model != null)
                    group.Children.Add(model);
            }
        }

        foreach (var child in node.VisualChildren)
            AddNode(child, group, materials);
    }

    private static GeometryModel3D? CreatePrimitive(
        MeshPrimitive primitive,
        Matrix4x4 world,
        Dictionary<int, WpfMaterial> materials)
    {
        var positions = primitive.GetVertexAccessor("POSITION")?.AsVector3Array();
        if (positions == null || positions.Count == 0)
            return null;

        var normals = primitive.GetVertexAccessor("NORMAL")?.AsVector3Array();
        var uvs = primitive.GetVertexAccessor("TEXCOORD_0")?.AsVector2Array();
        var mesh = new MeshGeometry3D();

        for (var i = 0; i < positions.Count; i++)
        {
            var position = Vector3.Transform(positions[i], world);
            mesh.Positions.Add(new Point3D(position.X, position.Y, position.Z));

            if (normals != null && i < normals.Count)
            {
                var normal = Vector3.Normalize(Vector3.TransformNormal(normals[i], world));
                mesh.Normals.Add(new Vector3D(normal.X, normal.Y, normal.Z));
            }

            // glTF and WPF both locate (0, 0) at the top-left of the image.
            if (uvs != null && i < uvs.Count)
                mesh.TextureCoordinates.Add(new System.Windows.Point(uvs[i].X, uvs[i].Y));
        }

        foreach (var (a, b, c) in primitive.GetTriangleIndices())
        {
            mesh.TriangleIndices.Add(a);
            mesh.TriangleIndices.Add(b);
            mesh.TriangleIndices.Add(c);
        }

        if (mesh.TriangleIndices.Count == 0)
            return null;

        var material = GetMaterial(primitive, materials);
        var model = new GeometryModel3D(mesh, material)
        {
            BackMaterial = material
        };



        return model;
    }

    private static WpfMaterial GetMaterial(MeshPrimitive primitive, Dictionary<int, WpfMaterial> materials)
    {
        var gltfMaterial = primitive.Material;
        if (gltfMaterial != null && materials.TryGetValue(gltfMaterial.LogicalIndex, out var cached))
            return cached;

        var material = BuildMaterial(gltfMaterial);
        if (gltfMaterial != null)
            materials[gltfMaterial.LogicalIndex] = material;
        return material;
    }

    private static WpfMaterial BuildMaterial(GltfMaterial? gltfMaterial)
    {
        var channel = gltfMaterial?.FindChannel("BaseColor");
        Brush brush = new SolidColorBrush(MediaColor.FromRgb(200, 200, 200));

        if (channel != null)
        {
            var image = channel.Value.Texture?.PrimaryImage;
            if (image != null && image.Content.IsValid)
            {
                brush = new ImageBrush(LoadBitmap(image.Content))
                {
                    Viewport = new System.Windows.Rect(0, 0, 1, 1),
                    ViewportUnits = BrushMappingMode.Absolute,
                    TileMode = TileMode.Tile,
                    Stretch = Stretch.Fill
                };
            }
            else
            {
                brush = new SolidColorBrush(MediaColor.FromRgb(200, 200, 200));
            }
        }

        brush.Freeze();
        var material = new DiffuseMaterial(brush);
        if (material.CanFreeze)
            material.Freeze();
        return material;
    }

    private static BitmapImage LoadBitmap(SharpGLTF.Memory.MemoryImage image)
    {
        using var source = image.Open();
        using var copy = new MemoryStream();
        source.CopyTo(copy);
        copy.Position = 0;

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = copy;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }
}
