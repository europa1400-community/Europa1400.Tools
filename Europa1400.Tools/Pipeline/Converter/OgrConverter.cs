using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Decoder.Ogr;
using Europa1400.Tools.Pipeline.Output;
using Newtonsoft.Json;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class OgrConverter : IConverter
    {
        public Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default)
        {
            if (!(input is OgrStruct ogr))
                throw new ArgumentException("Input is not of type OgrStruct");

            var json = JsonConvert.SerializeObject(new
            {
                ogr.Name,
                RawDataLength = ogr.RawData.Length,
                RawDataBase64 = Convert.ToBase64String(ogr.RawData)
            }, Formatting.Indented);

            var files = new List<IFileExport>
            {
                new FileExport
                {
                    FilePath = $"{ogr.Name}.json",
                    Content = System.Text.Encoding.UTF8.GetBytes(json)
                }
            };

            pipelineProgress.Current += 1;
            progress?.Report(pipelineProgress);

            return Task.FromResult(files.AsEnumerable());
        }
    }
}
