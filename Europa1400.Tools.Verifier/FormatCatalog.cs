using Europa1400.Tools.Gltf;
using Europa1400.Tools.Pipeline;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Converter;
using Europa1400.Tools.Pipeline.Decoder;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Bgf;
using Europa1400.Tools.Structs.Txs;

namespace Europa1400.Tools.Verifier;

internal static class FormatCatalog
{
    public static IReadOnlyList<FormatKind> All { get; } =
    [
        Create<GfxAsset, GfxDecoder>("Graphics",
            FromPipeline<GfxConverter>("png", "PNG sprites / font atlas", ExpandShapebanks),
            FromPipeline<GfxRawConverter>("bmp", "BMP (raw)", ExpandShapebanks)),
        Create<SbfAsset, SbfDecoder>("Sounds",
            FromPipeline<SbfConverter>("wav", "WAV"),
            FromPipeline<SbfRawConverter>("raw", "Raw WAV/MP3")),
        Create<AgebAsset, AgebDecoder>("Buildings"),
        Create<AobjAsset, AobjDecoder>("Objects"),
        Create<BgfAsset, BgfDecoder>("Models",
            new ConverterChoice
            {
                Id = "glb",
                DisplayName = "glTF (GLB)",
                Convert = ConvertBgfGlb
            },
            new ConverterChoice
            {
                Id = "obj",
                DisplayName = "Wavefront OBJ",
                Convert = ConvertBgfObj
            }),
        Create<BafAsset, BafDecoder>("Animations"),
        Create<BafIniAsset, BafIniDecoder>("Animation INI"),
        Create<Ed3Asset, Ed3Decoder>("Scenes", FromPipeline<Ed3Converter>("json", "JSON")),
        Create<OgrAsset, OgrDecoder>("OGR", FromPipeline<OgrConverter>("json", "JSON")),
        Create<ResAsset, ResDecoder>("Strings", FromPipeline<ResConverter>("json", "JSON")),
        Create<SavAsset, SavDecoder>("Saves", FromPipeline<SavConverter>("bytes", "Decompressed bytes")),
        Create<ScrAsset, ScrDecoder>("Scripts", FromPipeline<ScrConverter>("json", "JSON")),
        Create<TxsAsset, TxsDecoder>("Texture maps"),
        Create<FormAsset, FormDecoder>("Forms")
    ];

    private static FormatKind Create<TAsset, TDecoder>(string category, params ConverterChoice[] converters)
        where TAsset : GameAsset
        where TDecoder : IDecoder, new()
    {
        return new FormatKind
        {
            Id = typeof(TAsset).Name,
            Category = category,
            Discover = Discover<TAsset>,
            Decode = (asset, token) => new TDecoder().DecodeAsync(asset, token),
            Converters = converters
        };
    }

    private static ConverterChoice FromPipeline<TConverter>(
        string id,
        string displayName,
        Func<object, IEnumerable<object>>? expand = null)
        where TConverter : IConverter, new()
    {
        return new ConverterChoice
        {
            Id = id,
            DisplayName = displayName,
            Convert = async (_, decoded, progress, token) =>
            {
                var converter = new TConverter();
                var inputs = expand?.Invoke(decoded) ?? [decoded];
                var exports = new List<IFileExport>();
                var pipelineProgress = new PipelineProgress { Step = PipelineStep.Convert };

                foreach (var input in inputs)
                {
                    token.ThrowIfCancellationRequested();
                    pipelineProgress.Total += converter.EstimateSteps(input);
                }

                progress?.Report(pipelineProgress);

                foreach (var input in inputs)
                {
                    token.ThrowIfCancellationRequested();
                    var converted = await converter.ConvertAsync(input, pipelineProgress, progress, token);
                    exports.AddRange(converted);
                }

                return exports;
            }
        };
    }

    private static IReadOnlyList<GameAsset> Discover<TAsset>(string gamePath) where TAsset : GameAsset
    {
        return AssetDiscovererRegistry.Get<TAsset>().DiscoverAllFromGame(gamePath).Cast<GameAsset>().ToList();
    }

    private static IEnumerable<object> ExpandShapebanks(object decoded)
    {
        if (decoded is string || decoded is not System.Collections.IEnumerable enumerable)
            return [decoded];

        return enumerable.Cast<object>().Where(item => item != null).ToList()!;
    }

    private static Task<IReadOnlyList<IFileExport>> ConvertBgfGlb(
        GameAsset asset, object decoded, IProgress<PipelineProgress>? progress, CancellationToken token)
    {
        return Task.Run(() =>
        {
            token.ThrowIfCancellationRequested();
            if (decoded is not BgfStruct bgf)
                throw new InvalidOperationException("Decoded BGF is not a BgfStruct.");

            progress?.Report(new PipelineProgress
            {
                Step = PipelineStep.Convert,
                Total = 2,
                Current = 0,
                FileName = "Extracting textures…"
            });

            var textures = BgfConverter.EnsureTextures(
                VerifierPaths.GameDirectory,
                Path.Combine(VerifierPaths.CacheRoot, "textures"));
            var txs = TryLoadTxs(asset);

            progress?.Report(new PipelineProgress
            {
                Step = PipelineStep.Convert,
                Total = 2,
                Current = 1,
                FileName = "Building GLB…"
            });

            var modelData = GltfUtil.GetModelData(bgf, null, txs, textures);
            var scene = GltfUtil.CreateModel(modelData);
            var name = Sanitize(Path.GetFileNameWithoutExtension(asset.RelativePath));

            var tempGlb = Path.Combine(Path.GetTempPath(), $"{name}-{Guid.NewGuid():N}.glb");
            byte[] content;
            try
            {
                scene.ToGltf2().SaveGLB(tempGlb);
                content = File.ReadAllBytes(tempGlb);
            }
            finally
            {
                if (File.Exists(tempGlb))
                    File.Delete(tempGlb);
            }

            progress?.Report(new PipelineProgress
            {
                Step = PipelineStep.Convert,
                Total = 2,
                Current = 2,
                FileName = $"{name}.glb"
            });

            IReadOnlyList<IFileExport> exports =
            [
                new FileExport
                {
                    FilePath = $"{name}.glb",
                    Content = content
                }
            ];
            return exports;
        }, token);
    }

    private static Task<IReadOnlyList<IFileExport>> ConvertBgfObj(
        GameAsset asset, object decoded, IProgress<PipelineProgress>? progress, CancellationToken token)
    {
        return Task.Run(() =>
        {
            token.ThrowIfCancellationRequested();
            if (decoded is not BgfStruct bgf)
                throw new InvalidOperationException("Decoded BGF is not a BgfStruct.");

            progress?.Report(new PipelineProgress
            {
                Step = PipelineStep.Convert,
                Total = 2,
                Current = 0,
                FileName = "Extracting textures…"
            });

            var textures = BgfConverter.EnsureTextures(
                VerifierPaths.GameDirectory,
                Path.Combine(VerifierPaths.CacheRoot, "textures"));
            var txs = TryLoadTxs(asset);
            var name = Sanitize(Path.GetFileNameWithoutExtension(asset.RelativePath));
            var temp = Path.Combine(Path.GetTempPath(), "europa1400-obj-" + Guid.NewGuid().ToString("N"));

            progress?.Report(new PipelineProgress
            {
                Step = PipelineStep.Convert,
                Total = 2,
                Current = 1,
                FileName = "Writing OBJ…"
            });

            try
            {
                var converter = new BgfWavefrontConverter { TargetDirectory = temp };
                converter.Convert(bgf, txs, textures, name);

                var exports = Directory.GetFiles(temp, "*", SearchOption.AllDirectories)
                    .Select(file => (IFileExport)new FileExport
                    {
                        FilePath = Path.GetRelativePath(temp, file).Replace('\\', '/'),
                        Content = File.ReadAllBytes(file)
                    })
                    .ToList();

                progress?.Report(new PipelineProgress
                {
                    Step = PipelineStep.Convert,
                    Total = 2,
                    Current = 2,
                    FileName = $"{name}.obj"
                });

                return (IReadOnlyList<IFileExport>)exports;
            }
            finally
            {
                try
                {
                    Directory.Delete(temp, true);
                }
                catch
                {
                    // temp cleanup is best-effort
                }
            }
        }, token);
    }

    internal static TxsStruct? TryLoadTxs(GameAsset asset)
    {
        var txsPath = Path.ChangeExtension(asset.FilePath, ".TXS");
        if (!File.Exists(txsPath))
            return null;

        using var stream = File.OpenRead(txsPath);
        using var reader = new BinaryReader(stream);
        return TxsStruct.FromBytes(reader);
    }

    private static string Sanitize(string name)
    {
        return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, '_'));
    }
}
