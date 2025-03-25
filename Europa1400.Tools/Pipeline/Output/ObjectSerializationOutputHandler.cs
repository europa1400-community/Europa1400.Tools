using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;
using Newtonsoft.Json;

namespace Europa1400.Tools.Pipeline.Output
{
    public class ObjectSerializationOutputHandler<T> : IOutputHandler<T, OutputHandlerOptions>
    {
        public Task WriteAsync(T output, IGameAsset asset, OutputHandlerOptions options,
            CancellationToken cancellationToken = default)
        {
            var fileName = Path.GetFileNameWithoutExtension(asset.FilePath) + ".json";
            var fullPath = Path.Combine(options.OutputRoot, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            if (!options.OverwriteExisting && File.Exists(fullPath))
                return Task.CompletedTask;

            var json = JsonConvert.SerializeObject(output, Formatting.Indented);
            return File.WriteAllTextAsync(fullPath, json, cancellationToken);
        }
    }
}