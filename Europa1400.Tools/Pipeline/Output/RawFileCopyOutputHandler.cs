using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Output
{
    public class RawFileCopyOutputHandler : IOutputHandler<IGameAsset, OutputHandlerOptions>
    {
        public Task WriteAsync(IGameAsset asset, IGameAsset _, OutputHandlerOptions options,
            CancellationToken cancellationToken = default)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            WriteRecursive(asset, visited, options, cancellationToken);
            return Task.CompletedTask;
        }

        private static void WriteRecursive(IGameAsset asset, HashSet<string> visited, OutputHandlerOptions options,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(asset.FilePath) || !File.Exists(asset.FilePath))
                return;

            if (!visited.Add(asset.FilePath))
                return;

            var fileName = Path.GetFileName(asset.FilePath);
            var destPath = Path.Combine(options.OutputRoot, fileName);

            Directory.CreateDirectory(options.OutputRoot);

            if (options.OverwriteExisting || !File.Exists(destPath)) File.Copy(asset.FilePath, destPath, true);

            var properties = asset.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                var value = prop.GetValue(asset);
                switch (value)
                {
                    case IGameAsset child:
                        WriteRecursive(child, visited, options, cancellationToken);
                        break;
                    case IEnumerable<IGameAsset> list:
                        foreach (var item in list)
                            WriteRecursive(item, visited, options, cancellationToken);
                        break;
                }
            }
        }
    }
}