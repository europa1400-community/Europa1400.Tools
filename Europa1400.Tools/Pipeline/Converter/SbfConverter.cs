using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs.Sbf;
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
                    SoundType.Mp3 => ConvertToWav(sound),
                    _ => throw new NotSupportedException($"Unsupported sound type: {def.SoundType}")
                };

                files.Add(new FileExport { FilePath = fileName, Content = wavBytes });
            }
        }

        return files;
    }

    private static string Sanitize(string name)
    {
        return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, '_'));
    }

    private static byte[] ConvertToWav(byte[] mp3Bytes)
    {
        using var input = new MemoryStream(mp3Bytes);
        using var reader = new Mp3FileReader(input);
        using var output = new MemoryStream();
        WaveFileWriter.WriteWavFileToStream(output, reader);
        return output.ToArray();
    }
}