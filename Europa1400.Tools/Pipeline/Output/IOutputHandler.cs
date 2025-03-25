using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Output
{
    public interface IOutputHandler<in TOutput, in TOptions>
        where TOptions : OutputHandlerOptions
    {
        Task WriteAsync(TOutput output, IGameAsset asset, TOptions options,
            CancellationToken cancellationToken = default);
    }
}