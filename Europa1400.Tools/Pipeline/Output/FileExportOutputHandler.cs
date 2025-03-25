using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Output
{
    public class FileExportOutputHandler : IOutputHandler<List<IFileExport>, OutputHandlerOptions>
    {
        public Task WriteAsync(List<IFileExport> files, IGameAsset asset, OutputHandlerOptions options,
            CancellationToken cancellationToken = default)
        {
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fullPath = Path.Combine(options.OutputRoot, file.FilePath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                if (!options.OverwriteExisting && File.Exists(fullPath))
                    continue;

                File.WriteAllBytes(fullPath, file.Content);
            }

            return Task.CompletedTask;
        }
    }
}