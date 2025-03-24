namespace Europa1400.Tools.Pipeline.Converter;

public interface IConverter<in TDecoded, out TOutput>
{
    TOutput Convert(TDecoded input);
}