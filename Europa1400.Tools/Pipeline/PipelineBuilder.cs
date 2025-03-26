using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Converter;
using Europa1400.Tools.Pipeline.Decoder;
using Europa1400.Tools.Pipeline.Output;

namespace Europa1400.Tools.Pipeline
{
    public class PipelineBuilder<TAsset> where TAsset : GameAsset
    {
        private IConverter? _converter;
        private IDecoder? _decoder;
        private OutputHandlerOptions? _outputHandlerOptions;
        private bool _write;

        public static PipelineBuilder<TAsset> Create()
        {
            return new PipelineBuilder<TAsset>();
        }

        public PipelineBuilder<TAsset> DecodeWith<TDecoder>()
            where TDecoder : IDecoder, new()
        {
            _decoder = new TDecoder();
            return this;
        }

        public PipelineBuilder<TAsset> ConvertWith<TConverter>()
            where TConverter : IConverter, new()
        {
            _converter = new TConverter();
            return this;
        }

        public PipelineBuilder<TAsset> Write(OutputHandlerOptions? options = null)
        {
            _write = true;
            _outputHandlerOptions = options;
            return this;
        }

        public Pipeline<TAsset> Build()
        {
            return new Pipeline<TAsset>(_decoder, _converter, _write, _outputHandlerOptions);
        }
    }
}