using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Europa1400.Tools.Extensions;
using Europa1400.Tools.Pipeline;
using Europa1400.Tools.Structs.Baf;
using Europa1400.Tools.Structs.Bgf;

namespace Europa1400.Tools.Animation;

public sealed class BafAnimationClip
{
    public required string FilePath { get; init; }
    public required string RelativePath { get; init; }
    public required int[] ObjectVertexCounts { get; init; }
    public string Name => Path.GetFileNameWithoutExtension(FilePath);
    public string? Family { get; init; }
    public bool LowDetail { get; init; }

    public BafStruct Decode()
    {
        using var reader = new BinaryReader(File.OpenRead(FilePath));
        return BafStruct.FromBytes(reader);
    }
}

/// <summary>Matches animation families and ordered mesh layouts before binding vertices.</summary>
public sealed class BgfAnimationCatalog
{
    public IReadOnlyList<BafAnimationClip> Clips { get; }
    private BgfAnimationCatalog(IReadOnlyList<BafAnimationClip> clips) => Clips = clips;

    public static BgfAnimationCatalog FromGameInstallation(string gamePath, string cacheRoot,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(cacheRoot, "animations");
        ExtractionHelper.EnsureExtracted(Path.Combine(gamePath, "Resources", "animations.bin"), directory);
        return FromDirectory(directory, cancellationToken);
    }

    public static BgfAnimationCatalog FromDirectory(string directory, CancellationToken cancellationToken = default)
    {
        var clips = new List<BafAnimationClip>();
        foreach (var path in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                     .Where(p => Path.GetExtension(p).Equals(".baf", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var reader = new BinaryReader(File.OpenRead(path));
            var header = BafStruct.ReadHeader(reader);
            var counts = new int[checked((int)header.ObjectCount)];
            for (var i = 0; i < counts.Length; i++)
            {
                reader.SkipRequiredByte(0x18);
                reader.ReadUInt32();
                reader.SkipRequiredByte(0x19);
                counts[i] = checked((int)reader.ReadUInt32());
                reader.SkipRequiredByte(0x21);
                var bytes = checked((long)counts[i] * 12);
                if (bytes > reader.BaseStream.Length - reader.BaseStream.Position)
                    throw new InvalidDataException($"Incomplete animation: {path}");
                reader.BaseStream.Seek(bytes, SeekOrigin.Current);
                reader.SkipRequiredByte(0x28);
            }
            var relative = Path.GetRelativePath(directory, path).Replace('\\', '/');
            var parts = relative.Split('/');
            var familyIndex = Array.FindIndex(parts, p =>
                p.Equals("Character", StringComparison.OrdinalIgnoreCase) ||
                p.Equals("LowPolyCharacter", StringComparison.OrdinalIgnoreCase) ||
            p.Equals("Low_Poly_Character", StringComparison.OrdinalIgnoreCase));
            clips.Add(new BafAnimationClip
            {
                FilePath = path, RelativePath = relative, ObjectVertexCounts = counts,
                Family = familyIndex >= 0 && familyIndex + 2 < parts.Length ? parts[familyIndex + 1] : null,
                LowDetail = familyIndex >= 0 && parts[familyIndex].Equals("LowPolyCharacter", StringComparison.OrdinalIgnoreCase)
            });
        }
        return new BgfAnimationCatalog(clips);
    }

    public IReadOnlyList<BafAnimationClip> FindForModel(BgfStruct bgf, string modelPath)
    {
        var layout = bgf.GameObjects.Where(g => g.Model != null).Select(g => g.Model!.Vertices.Length).ToArray();
        if (layout.Sum() != bgf.MappingObject.VertexMappings.Length) return [];
        var stem = Path.GetFileNameWithoutExtension(modelPath).ToUpperInvariant();
        var tokens = stem.Split('_', StringSplitOptions.RemoveEmptyEntries);
        var isCharacter = modelPath.Replace('\\', '/').Split('/').Any(p =>
            p.Equals("Character", StringComparison.OrdinalIgnoreCase) ||
            p.Equals("LowPolyCharacter", StringComparison.OrdinalIgnoreCase) ||
            p.Equals("Low_Poly_Character", StringComparison.OrdinalIgnoreCase));
        return Clips.Where(clip =>
        {
            if (!layout.SequenceEqual(clip.ObjectVertexCounts)) return false;
            if (clip.Family != null)
            {
                if (!isCharacter) return false;
                return tokens.Contains(clip.Family.ToUpperInvariant()) && clip.LowDetail == tokens.Contains("LOW");
            }
            if (isCharacter) return false;
            var animationTokens = clip.Name.ToUpperInvariant().Split('_', StringSplitOptions.RemoveEmptyEntries);
            var modelNumbers = tokens.Where(t => t.All(char.IsDigit)).ToArray();
            var clipNumbers = animationTokens.Where(t => t.All(char.IsDigit)).ToArray();
            if (modelNumbers.Length > 0 && clipNumbers.Length > 0 &&
                !modelNumbers.SequenceEqual(clipNumbers)) return false;
            return tokens.Any(t => t.Length >= 4 && animationTokens.Contains(t));
        }).ToArray();
    }

    public static bool HasCompatibleLayout(BgfStruct bgf, BafStruct baf)
    {
        var layout = bgf.GameObjects.Where(g => g.Model != null).Select(g => g.Model!.Vertices.Length).ToArray();
        return layout.Sum() == bgf.MappingObject.VertexMappings.Length &&
               baf.VertexCount == bgf.MappingObject.VertexMappings.Length &&
               baf.Frames.Length > 0 && baf.Frames.All(f => layout.SequenceEqual(f.ObjectVertexCounts));
    }
}
