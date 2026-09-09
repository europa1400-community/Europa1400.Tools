using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Res;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class ResConverter : IConverter
    {
        public Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default)
        {
            if (input is not ResStruct res)
                throw new ArgumentException("Input is not of type ResStruct");

            pipelineProgress.Current += 1;
            progress?.Report(pipelineProgress);

            return Task.FromResult(FileExportFactory.Json("strings.json", res));
        }
    }
}
