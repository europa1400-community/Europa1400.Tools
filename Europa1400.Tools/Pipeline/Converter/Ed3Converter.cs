using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Ed3;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class Ed3Converter : IConverter
    {
        public Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default)
        {
            if (input is not Ed3Struct ed3)
                throw new ArgumentException("Input is not of type Ed3Struct");

            pipelineProgress.Current += 1;
            progress?.Report(pipelineProgress);

            return Task.FromResult(FileExportFactory.Json("scene.json", ed3));
        }
    }
}
