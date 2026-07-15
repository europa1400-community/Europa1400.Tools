using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Decoder.Scr;
using Europa1400.Tools.Pipeline.Output;
using Newtonsoft.Json;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class ScrConverter : IConverter
    {
        public Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default)
        {
            if (!(input is ScrStruct scr))
                throw new ArgumentException("Input is not of type ScrStruct");

            var positions = scr.Triplets.Select(t => new { t.X, t.Y, t.Z }).ToList();
            var json = JsonConvert.SerializeObject(new
            {
                TripletCount = scr.Triplets.Count,
                Positions = positions
            }, Formatting.Indented);

            var files = new List<IFileExport>
            {
                new FileExport
                {
                    FilePath = "coordinates.json",
                    Content = System.Text.Encoding.UTF8.GetBytes(json)
                }
            };

            pipelineProgress.Current += 1;
            progress?.Report(pipelineProgress);

            return Task.FromResult(files.AsEnumerable());
        }
    }
}
