using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Sbf;
using MP3Sharp;
using NAudio.Wave;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class SbfConverter : IConverter
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
                    pipelineProgress.FileName = Path.Combine($"{bankName}_{i:D2}.wav");
                    progress?.Report(pipelineProgress);
                    cancellationToken.ThrowIfCancellationRequested();

                    var sound = soundbank.Sounds[i];
                    var def = soundbank.SoundDefinitions[i];

                    var fileName = $"{bankName}_{i:D2}.wav";

                    var wavBytes = def.SoundType switch
                    {
                        SoundType.Wav => sound,
                        SoundType.Mp3 => ConvertMp3ToWav(sound),
                        _ => throw new NotSupportedException($"Unsupported sound type: {def.SoundType}")
                    };

                    files.Add(new FileExport
                    {
                        FilePath = fileName,
                        Content = wavBytes
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

        private static byte[] ConvertMp3ToWav(byte[] mp3Bytes)
        {
            using var mp3Stream = new MP3Stream(new MemoryStream(mp3Bytes));
            using var waveStream = new RawSourceWaveStream(
                mp3Stream,
                new WaveFormat(mp3Stream.Frequency, 16, mp3Stream.ChannelCount)
            );

            using var memoryStream = new MemoryStream();
            WaveFileWriter.WriteWavFileToStream(memoryStream, waveStream);
            return memoryStream.ToArray();
        }
    }
}