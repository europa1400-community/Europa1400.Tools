using System.IO;

namespace Europa1400.Tools.Pipeline
{
    public static class PipelineSettings
    {
        public static string CacheRoot { get; set; } = Path.GetFullPath("./cache");
    }
}