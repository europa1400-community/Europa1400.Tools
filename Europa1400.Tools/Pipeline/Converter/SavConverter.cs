using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Decoder.Sav;
using Europa1400.Tools.Pipeline.Output;
using Newtonsoft.Json;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class SavConverter : IConverter
    {
        public Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default)
        {
            if (!(input is SavStruct sav))
                throw new ArgumentException("Input is not of type SavStruct");

            var files = new List<IFileExport>
            {
                new FileExport
                {
                    FilePath = "savegame.bytes",
                    Content = sav.DecompressedData
                },
                new FileExport
                {
                    FilePath = "savegame_meta.json",
                    Content = System.Text.Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(new
                        {
                            sav.CompressedSize,
                            sav.DecompressedSize
                        }, Formatting.Indented))
                }
            };

            pipelineProgress.Current += 1;
            progress?.Report(pipelineProgress);

            return Task.FromResult(files.AsEnumerable());
        }
    }
}
