using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Output
{
    public class DummyOutputHandler : IOutputHandler<List<IFileExport>, OutputHandlerOptions>
    {
        public async Task WriteAsync(
            List<IFileExport> output,
            IGameAsset asset,
            OutputHandlerOptions options,
            CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }
    }
}