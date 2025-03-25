using System.Threading;
using System.Threading.Tasks;

namespace Europa1400.Tools.Pipeline.Converter
{
    public interface IConverter<in TInput, TOutput>
    {
        Task<TOutput> ConvertAsync(TInput input, CancellationToken cancellationToken = default);
    }
}