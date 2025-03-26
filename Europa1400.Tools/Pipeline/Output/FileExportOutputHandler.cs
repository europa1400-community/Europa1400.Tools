using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Output
{
    public class FileExportOutputHandler : IOutputHandler
    {
        public Task WriteAsync(object output, GameAsset asset, OutputHandlerOptions options,
            CancellationToken cancellationToken = default)
        {
            if (!(output is IFileExport fileExport))
                throw new InvalidOperationException("Output must be of type IFileExport");

            cancellationToken.ThrowIfCancellationRequested();

            var relativeDirectory = Path.GetDirectoryName(asset.RelativePath) ?? string.Empty;
            var fullPath = Path.Combine(options.OutputRoot, relativeDirectory, fileExport.FilePath);
            
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            if (!options.OverwriteExisting && File.Exists(fullPath))
                return Task.CompletedTask;

            File.WriteAllBytes(fullPath, fileExport.Content);

            return Task.CompletedTask;
        }
    }
}