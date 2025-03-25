using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline
{
    public class Pipeline<TAsset> where TAsset : IGameAsset
    {
        private readonly Func<object, CancellationToken, Task<object?>>? _convert;
        private readonly Func<TAsset, CancellationToken, Task<object?>>? _decode;
        private readonly Func<object, TAsset, CancellationToken, Task>? _write;

        public Pipeline(
            Func<TAsset, CancellationToken, Task<object?>>? decode,
            Func<object, CancellationToken, Task<object?>>? convert,
            Func<object, TAsset, CancellationToken, Task>? write)
        {
            _decode = decode;
            _convert = convert;
            _write = write;
        }

        public async Task ExecuteAsync(
            AssetSelection<TAsset> selection,
            IProgress<PipelineProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var total = selection.Assets.Count;
            var current = 0;

            foreach (var asset in selection.Assets)
            {
                object stage = asset;

                if (_decode != null)
                    stage = await _decode(asset, cancellationToken);

                if (_convert != null)
                    stage = await _convert(stage, cancellationToken);

                if (_write != null)
                    await _write(stage, asset, cancellationToken);

                current++;
                progress?.Report(new PipelineProgress
                {
                    TotalSteps = total,
                    CurrentStep = current,
                    Message = $"Processed {asset.FilePath}"
                });
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