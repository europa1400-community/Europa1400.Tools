namespace Europa1400.Tools.Pipeline.Output;

public class FileExport : IFileExport
{
    public required string FilePath { get; init; }
    public required byte[] Content { get; init; }
}