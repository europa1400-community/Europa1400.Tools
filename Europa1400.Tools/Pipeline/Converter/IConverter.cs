using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Output;

namespace Europa1400.Tools.Pipeline.Converter
{
    public interface IConverter
    {
        Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default);

        int EstimateSteps(object input)
        {
            return 1;
        }
    }
}