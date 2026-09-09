using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Sav;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class SavConverter : IConverter
    {
        public Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default)
        {
            if (input is not SavStruct sav)
                throw new ArgumentException("Input is not of type SavStruct");

            pipelineProgress.Current += 1;
            progress?.Report(pipelineProgress);

            return Task.FromResult(FileExportFactory.Bytes("savegame.bytes", sav.DecompressedData)
                .Concat(FileExportFactory.Json("savegame_meta.json", new
                {
                    sav.CompressedSize,
                    sav.DecompressedSize
                })));
        }
    }
}
