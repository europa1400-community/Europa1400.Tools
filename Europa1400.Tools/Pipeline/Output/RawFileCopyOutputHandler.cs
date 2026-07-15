using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Output
{
    public class RawFileCopyOutputHandler : IOutputHandler
    {
        public Task WriteAsync(object output, GameAsset asset, OutputHandlerOptions options,
            CancellationToken cancellationToken = default)
        {
            if (!(output is GameAsset outputAsset))
                throw new ArgumentException("Output must be a GameAsset", nameof(output));

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            WriteRecursive(outputAsset, visited, options, cancellationToken);
            return Task.CompletedTask;
        }

        private static void WriteRecursive(GameAsset asset, HashSet<string> visited, OutputHandlerOptions options,
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
                    case GameAsset child:
                        WriteRecursive(child, visited, options, cancellationToken);
                        break;
                    case IEnumerable<GameAsset> list:
                        foreach (var item in list)
                            WriteRecursive(item, visited, options, cancellationToken);
                        break;
                }
            }
        }
    }
}