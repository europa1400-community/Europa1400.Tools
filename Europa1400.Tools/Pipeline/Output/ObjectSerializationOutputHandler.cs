using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Assets;
using Newtonsoft.Json;

namespace Europa1400.Tools.Pipeline.Output
{
    public class ObjectSerializationOutputHandler : IOutputHandler
    {
        public Task WriteAsync(object output, GameAsset asset, OutputHandlerOptions options,
            CancellationToken cancellationToken = default)
        {
            var fullPath = Path.Combine(options.OutputRoot, Path.ChangeExtension(asset.RelativePath, "json"));

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            if (!options.OverwriteExisting && File.Exists(fullPath))
                return Task.CompletedTask;

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            var json = JsonConvert.SerializeObject(output, Formatting.Indented, settings);

            return File.WriteAllTextAsync(fullPath, json, cancellationToken);
        }
    }
}