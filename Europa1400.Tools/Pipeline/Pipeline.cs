using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Converter;
using Europa1400.Tools.Pipeline.Decoder;
using Europa1400.Tools.Pipeline.Output;

namespace Europa1400.Tools.Pipeline
{
    public class Pipeline<TAsset> where TAsset : GameAsset
    {
        private readonly IConverter? _converter;
        private readonly IDecoder? _decoder;
        private readonly OutputHandlerOptions _outputHandlerOptions;
        private readonly bool _write;

        public Pipeline(
            IDecoder? decoder = null,
            IConverter? converter = null,
            bool write = false,
            OutputHandlerOptions? outputHandlerOptions = null)
        {
            _decoder = decoder;
            _converter = converter;
            _write = write;
            _outputHandlerOptions = outputHandlerOptions ?? new OutputHandlerOptions();
        }

        public async Task ExecuteAsync(
            AssetSelection<TAsset> selection,
            IProgress<PipelineProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var pipelineProgress = new PipelineProgress();
            progress?.Report(pipelineProgress);

            var decodedAssets = await DecodeAsync(selection, pipelineProgress, progress, cancellationToken);

            if (_converter != null || _write)
                await ConvertAndWriteAsync(decodedAssets, typeof(TAsset).Name, progress, cancellationToken);
        }

        private async Task<IEnumerable<GameAsset>> DecodeAsync(
            AssetSelection<TAsset> selection,
            PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? decodeProgress = null,
            CancellationToken cancellationToken = default)
        {
            if (_decoder == null) return Enumerable.Empty<GameAsset>();

            pipelineProgress.Step = PipelineStep.Decode;
            pipelineProgress.Total = selection.Assets.Count;
            decodeProgress?.Report(pipelineProgress);

            var decodedAssets = new List<GameAsset>();

            foreach (var asset in selection.Assets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                pipelineProgress.Asset = asset;
                decodeProgress?.Report(pipelineProgress);

                var decoded = await _decoder.DecodeAsync(asset, cancellationToken);

                var cachePath = PathHelper.GetDecodedCacheBasePath(asset);
                var options = new OutputHandlerOptions
                {
                    OutputRoot = cachePath,
                    OverwriteExisting = true
                };

                var writer = new ObjectSerializationOutputHandler();

                if (_decoder is ISubAssetProvider subAssetProvider && decoded is IEnumerable<object> decodedEnumerable)
                {
                    var subAssets = subAssetProvider.GetSubAssets(asset, decoded).ToList();
                    await writer.WriteAsync(decodedEnumerable, subAssets, options, cancellationToken);
                    decodedAssets.AddRange(subAssets);
                }
                else
                {
                    var decodedFilePath = PathHelper.GetDecodedCachePath(asset, typeof(TAsset).Name);
                    var decodedRelativePath = PathHelper.GetDecodedCacheRelativePath(asset);
                    var decodedAsset = new GameAsset(decodedFilePath, decodedRelativePath);
                    await writer.WriteAsync(decoded, asset, options, cancellationToken);
                    decodedAssets.Add(decodedAsset);
                }


                pipelineProgress.Current += 1;
                decodeProgress?.Report(pipelineProgress);
            }

            return decodedAssets;
        }

        private async Task ConvertAndWriteAsync(
            IEnumerable<GameAsset> assets,
            string typeFolder,
            IProgress<PipelineProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            // Calculate total steps
            // When convert exists, we need to use the EstimateTotalSteps method from the decoder on each asset and get the sum
            // When convert does not exist, we need use the total number of assets

            var assetsList = assets.ToList();
            var pipelineProgress = new PipelineProgress();

            if (_converter != null && _decoder != null)
                foreach (var asset in assetsList)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var decoded = await LoadCachedDecodedAsset(asset, typeFolder, cancellationToken);
                    var steps = _converter.EstimateSteps(decoded);

                    pipelineProgress.Total += steps;
                }
            else
                pipelineProgress.Total = assetsList.Count;

            foreach (var asset in assetsList)
            {
                cancellationToken.ThrowIfCancellationRequested();
                pipelineProgress.Asset = asset;
                pipelineProgress.Step = _converter != null ? PipelineStep.Convert : PipelineStep.Write;
                progress?.Report(pipelineProgress);

                var objectsToWrite = new List<object>();

                if (_converter != null)
                    if (_decoder != null)
                    {
                        var decoded = await LoadCachedDecodedAsset(asset, typeFolder, cancellationToken);
                        var converted =
                            await _converter.ConvertAsync(decoded, pipelineProgress, progress, cancellationToken);
                        objectsToWrite.AddRange(converted);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                if (_write)
                {
                    if (_decoder == null && _converter == null)
                    {
                        objectsToWrite.Add(asset);
                    }
                    else if (_decoder != null && _converter == null)
                    {
                        var decoded = await LoadCachedDecodedAsset(asset, typeFolder, cancellationToken);
                        objectsToWrite.Add(decoded);
                    }
                    else if (_decoder == null && _converter != null)
                    {
                        throw new NotImplementedException();
                    }

                    foreach (var objectToWrite in objectsToWrite)
                    {
                        await WriteAsync(objectToWrite, asset, cancellationToken);

                        if (_converter == null)
                        {
                            pipelineProgress.Current += 1;
                            progress?.Report(pipelineProgress);
                        }
                    }
                }
            }
        }

        private async Task<object> LoadCachedDecodedAsset(GameAsset asset, string typeFolder,
            CancellationToken cancellationToken)
        {
            var cachePath = PathHelper.GetDecodedCachePath(asset, typeFolder);

            if (!File.Exists(cachePath))
                throw new FileNotFoundException($"Decoded asset not found: {cachePath}");

            return await ObjectSerializationInputLoader.LoadAsync(cachePath, cancellationToken);
        }

        private async Task WriteAsync(object objectToWrite, GameAsset asset, CancellationToken cancellationToken)
        {
            switch (objectToWrite)
            {
                case IFileExport fileExport:
                {
                    var fileExportOutputHandler = new FileExportOutputHandler();
                    await fileExportOutputHandler.WriteAsync(fileExport, asset, _outputHandlerOptions,
                        cancellationToken);
                    break;
                }
                case GameAsset gameAsset:
                {
                    var rawFileCopyOutputHandler = new RawFileCopyOutputHandler();
                    await rawFileCopyOutputHandler.WriteAsync(gameAsset, asset, _outputHandlerOptions,
                        cancellationToken);
                    break;
                }
                default:
                {
                    var objectSerializationOutputHandler = new ObjectSerializationOutputHandler();
                    await objectSerializationOutputHandler.WriteAsync(objectToWrite, asset, _outputHandlerOptions,
                        cancellationToken);
                    break;
                }
            }
        }

        public Task ExecuteAsync(TAsset asset)
        {
            return ExecuteAsync(new AssetSelection<TAsset>(new List<TAsset> { asset }.AsReadOnly()));
        }

        public Task ExecuteAsync(IEnumerable<TAsset> assets)
        {
            return ExecuteAsync(new AssetSelection<TAsset>(assets));
        }
    }
}