using System.Text.Json;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Output;

public class ObjectSerializationOutputHandler<T>(string outputRoot) : IOutputHandler<T>
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public void Write(T output, IGameAsset asset)
    {
        var fileName = Path.GetFileNameWithoutExtension(asset.FilePath) + ".json";
        var fullPath = Path.Combine(outputRoot, fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var json = JsonSerializer.Serialize(output, _jsonOptions);

        File.WriteAllText(fullPath, json);
    }
}