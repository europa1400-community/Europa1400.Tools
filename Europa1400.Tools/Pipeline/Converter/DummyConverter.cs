using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Output;

namespace Europa1400.Tools.Pipeline.Converter
{
    internal class DummyConverter : IConverter
    {
        public async Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            pipelineProgress.Current += 1;
            progress?.Report(pipelineProgress);
            return new List<IFileExport> { new FileExport() };
        }
    }
}