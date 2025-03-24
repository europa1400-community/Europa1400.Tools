using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Sbf;
using MP3Sharp;
using NAudio.Wave;

namespace Europa1400.Tools.Pipeline.Converter;

public class SbfConverter : IConverter<SbfStruct, List<IFileExport>>
{
    public List<IFileExport> Convert(SbfStruct input)
    {
        var files = new List<IFileExport>();

        foreach (var soundbank in input.Soundbanks)
        {
            var bankName = Sanitize(soundbank.SoundbankDefinition.Name);

            for (var i = 0; i < soundbank.Sounds.Length; i++)
            {
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
            }
        }

        return files;
    }

    private static string Sanitize(string name)
    {
        return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, '_'));
    }

    private static byte[] ConvertMp3ToWav(byte[] mp3Bytes)
    {
        using var mp3Stream = new MP3Stream(new MemoryStream(mp3Bytes));
        using var waveStream =
            new RawSourceWaveStream(mp3Stream, new WaveFormat(mp3Stream.Frequency, 16, mp3Stream.ChannelCount));
        using var memoryStream = new MemoryStream();
        WaveFileWriter.WriteWavFileToStream(memoryStream, waveStream);
        return memoryStream.ToArray();
    }
}