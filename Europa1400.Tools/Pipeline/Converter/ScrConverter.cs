using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Scr;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class ScrConverter : IConverter
    {
        public Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default)
        {
            if (input is not ScrStruct scr)
                throw new ArgumentException("Input is not of type ScrStruct");

            pipelineProgress.Current += 1;
            progress?.Report(pipelineProgress);

            return Task.FromResult(FileExportFactory.Json("coordinates.json", scr));
        }
    }
}
