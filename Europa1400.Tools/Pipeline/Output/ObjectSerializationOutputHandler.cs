using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (output is IEnumerable<object> enumerable)
                return Task.WhenAll(enumerable.AsEnumerable()
                    .Select(e => WriteAsync(e, asset, options, cancellationToken)));

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

        public Task WriteAsync(IEnumerable<object> output, IEnumerable<GameAsset> assets, OutputHandlerOptions options,
            CancellationToken cancellationToken = default)
        {
            var outputList = output.ToList();
            var assetList = assets.ToList();

            if (outputList.Count != assetList.Count)
                throw new InvalidDataException("Output count does not match asset count");

            return Task.WhenAll(outputList
                .Zip(assetList, (o, a) => WriteAsync(o, a, options, cancellationToken)));
        }
    }
}