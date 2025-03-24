using System.IO;
using Europa1400.Tools.Pipeline.Assets;
using Newtonsoft.Json;

namespace Europa1400.Tools.Pipeline.Output
{
    public class ObjectSerializationOutputHandler<T> : IOutputHandler<T>
    {
        private readonly string outputRoot;

        public ObjectSerializationOutputHandler(string outputRoot)
        {
            this.outputRoot = outputRoot;
        }

        public void Write(T output, IGameAsset asset)
        {
            var fileName = Path.GetFileNameWithoutExtension(asset.FilePath) + ".json";
            var fullPath = Path.Combine(outputRoot, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            
            var json = JsonConvert.SerializeObject(output, Formatting.Indented);

            File.WriteAllText(fullPath, json);
        }
    }
}