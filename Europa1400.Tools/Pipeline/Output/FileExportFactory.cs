using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Europa1400.Tools.Pipeline.Output
{
    internal static class FileExportFactory
    {
        public static IEnumerable<IFileExport> Json(string filePath, object value)
        {
            return new[]
            {
                new FileExport
                {
                    FilePath = filePath,
                    Content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value, Formatting.Indented))
                }
            };
        }

        public static IEnumerable<IFileExport> Bytes(string filePath, byte[] content)
        {
            return new[]
            {
                new FileExport
                {
                    FilePath = filePath,
                    Content = content
                }
            };
        }
    }
}
