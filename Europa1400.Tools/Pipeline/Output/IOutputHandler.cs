using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Output
{
    public interface IOutputHandler
    {
        Task WriteAsync(
            object output,
            GameAsset asset,
            OutputHandlerOptions options,
            CancellationToken cancellationToken = default);
    }
}