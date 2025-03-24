using System.Reflection;
using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Output;

public class RawFileCopyOutputHandler(string outputRoot) : IOutputHandler<IGameAsset>
{
    public void Write(IGameAsset asset, IGameAsset _)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        WriteRecursive(asset, visited);
    }

    private void WriteRecursive(IGameAsset asset, HashSet<string> visited)
    {
        if (string.IsNullOrWhiteSpace(asset.FilePath))
            return;

        if (!File.Exists(asset.FilePath))
            return;

        if (!visited.Add(asset.FilePath))
            return;

        var fileName = Path.GetFileName(asset.FilePath);
        var destPath = Path.Combine(outputRoot, fileName);

        Directory.CreateDirectory(outputRoot);
        File.Copy(asset.FilePath, destPath, true);

        var properties = asset.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(asset);
            switch (value)
            {
                case IGameAsset child:
                    WriteRecursive(child, visited);
                    break;
                case IEnumerable<IGameAsset> list:
                {
                    foreach (var item in list)
                        WriteRecursive(item, visited);
                    break;
                }
            }
        }
    }
}