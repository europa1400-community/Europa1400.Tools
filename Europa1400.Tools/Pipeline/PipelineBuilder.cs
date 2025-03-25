using System;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Converter;
using Europa1400.Tools.Pipeline.Decoder;
using Europa1400.Tools.Pipeline.Output;

namespace Europa1400.Tools.Pipeline
{
    public class PipelineBuilder<TAsset> where TAsset : IGameAsset
    {
        private Func<object, CancellationToken, Task<object?>>? _convert;
        private Func<TAsset, CancellationToken, Task<object?>>? _decode;
        private Func<object, TAsset, CancellationToken, Task>? _write;

        public static PipelineBuilder<TAsset> Create()
        {
            return new PipelineBuilder<TAsset>();
        }

        public PipelineBuilder<TAsset> DecodeWith<TDecoder, TDecoded>()
            where TDecoder : IDecoder<TAsset, TDecoded>, new()
        {
            var decoder = new TDecoder();
            _decode = async (asset, ct) => await decoder.DecodeAsync(asset, ct);
            return this;
        }

        public PipelineBuilder<TAsset> ConvertWith<TConverter, TInput, TOutput>()
            where TConverter : IConverter<TInput, TOutput>, new()
        {
            var converter = new TConverter();
            _convert = async (input, ct) => await converter.ConvertAsync((TInput)input, ct);
            return this;
        }

        public PipelineBuilder<TAsset> WriteWith<THandler, TOutput, TOptions>(TOptions options)
            where THandler : IOutputHandler<TOutput, TOptions>, new()
            where TOptions : OutputHandlerOptions
        {
            var handler = new THandler();

            _write = async (output, asset, ct) =>
            {
                if (output is TOutput typed)
                    await handler.WriteAsync(typed, asset, options, ct);
                else
                    throw new InvalidOperationException($"Expected output of type {typeof(TOutput).Name}.");
            };

            return this;
        }

        public PipelineBuilder<TAsset> WriteWith<THandler, TOutput>()
            where THandler : IOutputHandler<TOutput, OutputHandlerOptions>, new()
        {
            return WriteWith<THandler, TOutput>(new OutputHandlerOptions());
        }

        public PipelineBuilder<TAsset> WriteWith<THandler, TOutput>(OutputHandlerOptions options)
            where THandler : IOutputHandler<TOutput, OutputHandlerOptions>, new()
        {
            var handler = new THandler();

            _write = async (output, asset, ct) =>
            {
                if (output is TOutput typed)
                    await handler.WriteAsync(typed, asset, options, ct);
                else
                    throw new InvalidOperationException($"Expected output of type {typeof(TOutput).Name}.");
            };

            return this;
        }

        public Pipeline<TAsset> Build()
        {
            return new Pipeline<TAsset>(_decode, _convert, _write);
        }
    }
}