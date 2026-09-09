using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Ogr;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class OgrConverter : IConverter
    {
        public Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default)
        {
            if (input is not OgrStruct ogr)
                throw new ArgumentException("Input is not of type OgrStruct");

            pipelineProgress.Current += 1;
            progress?.Report(pipelineProgress);

            return Task.FromResult(FileExportFactory.Json($"{ogr.Name}.json", ogr));
        }
    }
}
