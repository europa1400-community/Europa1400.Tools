namespace Europa1400.Tools.Pipeline.Output
{
    public class FileExport : IFileExport
    {
        public string FilePath { get; set; }
        public byte[] Content { get; set; }
    }
}