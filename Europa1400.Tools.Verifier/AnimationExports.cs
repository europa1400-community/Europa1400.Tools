using Europa1400.Tools.Animation;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Converter;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Bgf;
using System.Security.Cryptography;
using System.Text;

namespace Europa1400.Tools.Verifier;

internal static class AnimationExports
{
    private static readonly Lazy<BgfAnimationCatalog> Catalog = new(() =>
        BgfAnimationCatalog.FromGameInstallation(VerifierPaths.GameDirectory, VerifierPaths.CacheRoot));

    public static IReadOnlyList<BafAnimationClip> Find(GameAsset asset, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        using var reader = new BinaryReader(File.OpenRead(asset.FilePath));
        var model = BgfStruct.FromBytes(reader);
        var clips = Catalog.Value.FindForModel(model, asset.RelativePath);
        token.ThrowIfCancellationRequested();
        return clips;
    }

    public static ConverterChoice ForClip(BafAnimationClip clip)
    {
        var info = new FileInfo(clip.FilePath);
        var stamp = $"{clip.RelativePath}|{info.Length}|{info.LastWriteTimeUtc.Ticks}";
        var id = System.Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(stamp)))[..20];
        return new ConverterChoice
        {
            Id = $"glb-animation-{id}",
            DisplayName = clip.Name,
            Convert = (asset, decoded, progress, token) => Task.Run<IReadOnlyList<IFileExport>>(() =>
            {
                token.ThrowIfCancellationRequested();
                var textures = BgfConverter.EnsureTextures(VerifierPaths.GameDirectory,
                    Path.Combine(VerifierPaths.CacheRoot, "textures"));
                var root = BgfAnimationConverter.Convert((BgfStruct)decoded, clip.Decode(), clip.Name,
                    textures, FormatCatalog.TryLoadTxs(asset), cancellationToken: token);
                var name = $"{Path.GetFileNameWithoutExtension(asset.FilePath)}--{clip.Name}.glb";
                var temp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.glb");
                try
                {
                    root.SaveGLB(temp);
                    token.ThrowIfCancellationRequested();
                    return [new FileExport { FilePath = name, Content = File.ReadAllBytes(temp) }];
                }
                finally { if (File.Exists(temp)) File.Delete(temp); }
            }, token)
        };
    }
}
