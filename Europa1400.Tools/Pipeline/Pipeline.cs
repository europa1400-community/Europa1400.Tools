using System;
using System.Collections.Generic;
using System.IO;
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

            await DecodeAsync(selection, pipelineProgress, progress, cancellationToken);

            if (_converter != null || _write)
                await ConvertAndWriteAsync(selection, pipelineProgress, progress, cancellationToken);
        }

        private async Task DecodeAsync(
            AssetSelection<TAsset> selection,
            PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? decodeProgress = null,
            CancellationToken cancellationToken = default)
        {
            if (_decoder == null) return;

            pipelineProgress.Step = PipelineStep.Decode;
            pipelineProgress.Total = selection.Assets.Count;
            decodeProgress?.Report(pipelineProgress);

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
                await writer.WriteAsync(decoded, asset, options, cancellationToken);

                pipelineProgress.Current += 1;
                decodeProgress?.Report(pipelineProgress);
            }
        }

        private async Task ConvertAndWriteAsync(
            AssetSelection<TAsset> selection,
            PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            // Calculate total steps
            // When convert exists, we need to use the EstimateTotalSteps method from the decoder on each asset and get the sum
            // When convert does not exist, we need use the total number of assets

            pipelineProgress = new PipelineProgress();

            if (_converter != null && _decoder != null)
                foreach (var asset in selection.Assets)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var decoded = await LoadCachedDecodedAsset(asset, cancellationToken);
                    var steps = _converter.EstimateSteps(decoded);

                    pipelineProgress.Total += steps;
                }
            else
                pipelineProgress.Total = selection.Assets.Count;

            foreach (var asset in selection.Assets)
            {
                cancellationToken.ThrowIfCancellationRequested();
                pipelineProgress.Asset = asset;
                pipelineProgress.Step = _converter != null ? PipelineStep.Convert : PipelineStep.Write;
                progress?.Report(pipelineProgress);

                var objectsToWrite = new List<object>();

                if (_converter != null)
                    if (_decoder != null)
                    {
                        var decoded = await LoadCachedDecodedAsset(asset, cancellationToken);
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
                        var decoded = await LoadCachedDecodedAsset(asset, cancellationToken);
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

        private async Task<object> LoadCachedDecodedAsset(TAsset asset, CancellationToken cancellationToken)
        {
            var cachePath = PathHelper.GetDecodedCachePath(asset);

            if (!File.Exists(cachePath))
                throw new FileNotFoundException($"Decoded asset not found: {cachePath}");

            return await ObjectSerializationInputLoader.LoadAsync(cachePath, cancellationToken);
        }

        private async Task WriteAsync(object objectToWrite, TAsset asset, CancellationToken cancellationToken)
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