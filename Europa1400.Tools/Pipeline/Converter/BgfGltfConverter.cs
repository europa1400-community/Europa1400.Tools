using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Output;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class BgfGltfConverter : IConverter
    {
        public string GamePath { get; set; }

        public int EstimateSteps(object input)
        {
            return 1;
        }

        public Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "BgfPipeline_" + Guid.NewGuid().ToString("N"));
            var converter = new BgfConverter
            {
                PathToGameFiles = GamePath,
                TargetDirectory = tempDir
            };

            converter.Convert();

            if (!Directory.Exists(tempDir))
                return Task.FromResult(Enumerable.Empty<IFileExport>());

            var exports = new List<IFileExport>();
            var convertedDir = Path.Combine(tempDir, "converted_objects");

            if (Directory.Exists(convertedDir))
            {
                foreach (var glbFile in Directory.GetFiles(convertedDir, "*.glb", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(convertedDir, glbFile).Replace('\\', '/');
                    exports.Add(new FileExport
                    {
                        FilePath = Path.Combine("models", relativePath),
                        Content = File.ReadAllBytes(glbFile)
                    });
                }
            }

            try { Directory.Delete(tempDir, true); }
            catch { /* cleanup best-effort */ }

            return Task.FromResult(exports.AsEnumerable());
        }
    }
}
