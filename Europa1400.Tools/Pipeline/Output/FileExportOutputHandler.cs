using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Pipeline.Output;

public class FileExportOutputHandler(string outputRoot) : IOutputHandler<List<IFileExport>>
{
    public void Write(List<IFileExport> output, IGameAsset asset)
    {
        foreach (var file in output)
        {
            var fullPath = Path.Combine(outputRoot, file.FilePath);
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(fullPath, file.Content);
        }
    }
}