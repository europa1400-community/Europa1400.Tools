using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Converter;
using Europa1400.Tools.Pipeline.Decoder;
using Europa1400.Tools.Pipeline.Output;

namespace Europa1400.Tools.Pipeline;

public class PipelineBuilder<TAsset>
    where TAsset : IGameAsset
{
    private Func<object, object>? _convert;
    private Func<TAsset, object>? _decode;
    private Action<object, TAsset>? _write;

    public static PipelineBuilder<TAsset> Create()
    {
        return new PipelineBuilder<TAsset>();
    }

    public PipelineBuilder<TAsset> DecodeWith<TDecoder, TDecoded>()
        where TDecoder : IDecoder<TAsset, TDecoded>, new()
    {
        var decoder = new TDecoder();
        _decode = asset => decoder.Decode(asset)!;
        return this;
    }

    public PipelineBuilder<TAsset> ConvertWith<TConverter, TInput, TOutput>()
        where TConverter : IConverter<TInput, TOutput>, new()
    {
        var converter = new TConverter();
        _convert = input => converter.Convert((TInput)input)!;
        return this;
    }

    public PipelineBuilder<TAsset> WriteTo(string outputPath)
    {
        if (_convert != null)
        {
            _write = (output, asset) =>
            {
                if (output is List<IFileExport> files)
                {
                    var writer = new FileExportOutputHandler(outputPath);
                    writer.Write(files, asset);
                }
                else
                {
                    throw new InvalidOperationException("WriteTo expected List<IFileExport> as output.");
                }
            };
        }
        else if (_decode != null)
        {
            _write = (decoded, asset) =>
            {
                var handlerType = typeof(ObjectSerializationOutputHandler<>).MakeGenericType(decoded.GetType());
                dynamic handler = Activator.CreateInstance(handlerType, outputPath)!;
                handler.Write((dynamic)decoded, asset);
            };
        }
        else
        {
            var copier = new RawFileCopyOutputHandler(outputPath);
            _write = (input, asset) => copier.Write((IGameAsset)input, asset);
        }

        return this;
    }

    public Pipeline<TAsset> Build()
    {
        return new Pipeline<TAsset>(_decode, _convert, _write);
    }
}