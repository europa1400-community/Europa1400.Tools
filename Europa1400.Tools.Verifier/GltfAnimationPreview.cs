using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using SharpGLTF.Schema2;
using GltfAnimation = SharpGLTF.Schema2.Animation;

namespace Europa1400.Tools.Verifier;

/// <summary>A mutable WPF view sampled from the animation embedded in a GLB.</summary>
internal sealed class GltfAnimationPreview
{
    private readonly ModelRoot _root;
    private readonly List<PrimitiveBinding> _bindings = [];
    public Model3DGroup Model { get; }
    public IReadOnlyList<GltfAnimation> Animations => _root.LogicalAnimations;

    internal GltfAnimationPreview(ModelRoot root, Model3DGroup model)
    {
        _root = root;
        Model = model;
        var index = 0;
        void Bind(Node node)
        {
            if (node.Mesh != null)
            {
                foreach (var primitive in node.Mesh.Primitives)
                {
                    if (primitive.GetVertexAccessor("POSITION") is not { Count: > 0 } ||
                        !primitive.GetTriangleIndices().Any()) continue;
                    var geometry = (MeshGeometry3D)((GeometryModel3D)model.Children[index++]).Geometry;
                    _bindings.Add(new PrimitiveBinding(node, primitive, geometry));
                }
            }
            foreach (var child in node.VisualChildren) Bind(child);
        }
        foreach (var node in (root.DefaultScene ?? root.LogicalScenes[0]).VisualChildren) Bind(node);
    }

    public float Duration(int animationIndex) => Animations[animationIndex].Duration;

    public (float Start, float End) LoopRange(int animationIndex)
    {
        var animation = Animations[animationIndex];
        var start = animation.Extras?["loopStart"]?.GetValue<float>() ?? 0;
        var end = animation.Extras?["loopEnd"]?.GetValue<float>() ?? animation.Duration;
        return start >= 0 && end > start && end <= animation.Duration
            ? (start, end) : (0, animation.Duration);
    }

    public void SetTime(int animationIndex, float time, bool inPlace)
    {
        var animation = Animations[animationIndex];
        time = Math.Clamp(time, 0, animation.Duration);
        foreach (var binding in _bindings) binding.Apply(animation, time, inPlace);
    }

    private sealed class PrimitiveBinding
    {
        private readonly Node _node;
        private readonly MeshGeometry3D _geometry;
        private readonly Vector3[] _positions;
        private readonly Vector3[]? _normals;
        private readonly Vector3[][] _positionDeltas;
        private readonly Vector3[]?[] _normalDeltas;
        private readonly bool _rootMotion;

        public PrimitiveBinding(Node node, MeshPrimitive primitive, MeshGeometry3D geometry)
        {
            _node = node;
            _geometry = geometry;
            _positions = primitive.GetVertexAccessor("POSITION").AsVector3Array().ToArray();
            _normals = primitive.GetVertexAccessor("NORMAL")?.AsVector3Array().ToArray();
            _positionDeltas = new Vector3[primitive.MorphTargetsCount][];
            _normalDeltas = new Vector3[]?[primitive.MorphTargetsCount];
            for (var i = 0; i < primitive.MorphTargetsCount; i++)
            {
                var target = primitive.GetMorphTargetAccessors(i);
                _positionDeltas[i] = target.TryGetValue("POSITION", out var p)
                    ? p.AsVector3Array().ToArray() : new Vector3[_positions.Length];
                _normalDeltas[i] = target.TryGetValue("NORMAL", out var n) ? n.AsVector3Array().ToArray() : null;
            }
            _rootMotion = node.Extras?["bgfRootMotion"]?.GetValue<bool>() == true;
        }

        public void Apply(GltfAnimation animation, float time, bool inPlace)
        {
            var samplers = _node.GetCurveSamplers(animation);
            var weights = samplers.HasMorphingCurves
                ? samplers.GetMorphingWeights<float[]>(time) : _node.MorphWeights;
            var active = Enumerable.Range(0, Math.Min(weights.Count, _positionDeltas.Length))
                .Where(i => Math.Abs(weights[i]) > 1e-7f).ToArray();
            var world = _node.GetWorldMatrix(animation, time);
            if (inPlace && _rootMotion) world.Translation = _node.WorldMatrix.Translation;
            var normalMatrix = Matrix4x4.Invert(world, out var inverse)
                ? Matrix4x4.Transpose(inverse) : world;
            var positions = new Point3DCollection(_positions.Length);
            var normals = _normals == null ? null : new Vector3DCollection(_positions.Length);
            for (var v = 0; v < _positions.Length; v++)
            {
                var position = _positions[v];
                var normal = _normals?[v] ?? Vector3.Zero;
                foreach (var i in active)
                {
                    position += _positionDeltas[i][v] * weights[i];
                    if (_normalDeltas[i] is { } deltas) normal += deltas[v] * weights[i];
                }
                position = Vector3.Transform(position, world);
                positions.Add(new Point3D(position.X, position.Y, position.Z));
                if (normals != null)
                {
                    normal = Vector3.TransformNormal(normal, normalMatrix);
                    if (normal.LengthSquared() > 1e-12f) normal = Vector3.Normalize(normal);
                    normals.Add(new Vector3D(normal.X, normal.Y, normal.Z));
                }
            }
            positions.Freeze();
            _geometry.Positions = positions;
            if (normals != null) { normals.Freeze(); _geometry.Normals = normals; }
        }
    }
}
