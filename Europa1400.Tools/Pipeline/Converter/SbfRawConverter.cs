using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Sbf;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class SbfRawConverter : IConverter
    {
        public int EstimateSteps(object input)
        {
            if (!(input is SbfStruct sbfStruct))
                throw new ArgumentException("Input is not of type SbfStruct");

            return sbfStruct.Soundbanks.Sum(e => e.Sounds.Length);
        }

        public Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default)
        {
            if (!(input is SbfStruct sbfStruct))
                throw new ArgumentException("Input is not of type SbfStruct");

            var files = new List<IFileExport>();

            foreach (var soundbank in sbfStruct.Soundbanks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var bankName = Sanitize(soundbank.SoundbankDefinition.Name);

                for (var i = 0; i < soundbank.Sounds.Length; i++)
                {
                    var ext = soundbank.SoundDefinitions[i].SoundType switch
                    {
                        SoundType.Wav => ".wav",
                        SoundType.Mp3 => ".mp3",
                        _ => ".bin"
                    };

                    var fileName = Path.Combine(sbfStruct.Name, $"{bankName}_{i:000}{ext}");
                    pipelineProgress.FileName = fileName;
                    progress?.Report(pipelineProgress);
                    cancellationToken.ThrowIfCancellationRequested();

                    files.Add(new FileExport
                    {
                        FilePath = fileName,
                        Content = soundbank.Sounds[i]
                    });

                    pipelineProgress.Current += 1;
                    progress?.Report(pipelineProgress);
                }
            }

            return Task.FromResult(files.AsEnumerable());
        }

        private static string Sanitize(string name)
        {
            return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, '_'));
        }
    }
}
