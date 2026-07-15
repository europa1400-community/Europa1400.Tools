using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Decoder.Res;
using Europa1400.Tools.Pipeline.Output;
using Newtonsoft.Json;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class ResConverter : IConverter
    {
        public Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default)
        {
            if (!(input is ResStruct res))
                throw new ArgumentException("Input is not of type ResStruct");

            var json = JsonConvert.SerializeObject(new
            {
                res.EntryCount,
                NameCount = res.NameStrings.Count,
                ValueCount = res.ValueStrings.Count,
                NameStrings = res.NameStrings,
                ValueStrings = res.ValueStrings
            }, Formatting.Indented);

            var files = new List<IFileExport>
            {
                new FileExport
                {
                    FilePath = "strings.json",
                    Content = System.Text.Encoding.UTF8.GetBytes(json)
                }
            };

            pipelineProgress.Current += 1;
            progress?.Report(pipelineProgress);

            return Task.FromResult(files.AsEnumerable());
        }
    }
}
