using Europa1400.Tools.Decoder.Sbf;
using Europa1400.Tools.Interfaces;
using NAudio.Wave;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Europa1400.Tools.Tests")]
namespace Europa1400.Tools.Converter;

internal class SoundConverter : IConverter
{
    public byte[] ConvertToWav(byte[] mp3Bytes)
    {
        using var inputStream = new MemoryStream(mp3Bytes);
        using var waveStream = new Mp3FileReader(inputStream);
        using var targetStream = new MemoryStream();
        WaveFileWriter.WriteWavFileToStream(targetStream, waveStream);

        return targetStream.ToArray();
    }

    public void Convert(string pathToGameFiles, string targetDirectory)
    {
        var sourceDirectory = new DirectoryInfo(pathToGameFiles);

        var outputPath = Path.Combine(targetDirectory, "Sound");
        var soundDirectory = new DirectoryInfo(outputPath);
        soundDirectory.Create();

        var soundFiles = sourceDirectory.GetFiles("*.sbf", SearchOption.AllDirectories);
        
        var soundGroups = new Dictionary<string, List<byte[]>>();

        #region Convert sounds to WAV and group into dictionary

        foreach (var soundFile in soundFiles)
        {
            using var br = new BinaryReader(File.OpenRead(soundFile.FullName));
            var sbfStruct = SbfStruct.FromBytes(br);

            var audioBytes = new List<byte[]>();

            for (int i = 0; i < sbfStruct.SoundbankCount; i++)
            {
                var soundbank = sbfStruct.Soundbanks.ElementAt(i);

                for (int j = 0; j < soundbank.SoundCount; j++)
                {
                    var sound = soundbank.Sounds.ElementAt(j);
                    var soundDefinition = soundbank.SoundDefinitions.ElementAt(j);

                    if (soundDefinition.SoundType == Enums.SoundType.WAV)
                    {
                        audioBytes.Add(sound.ToArray());
                    }
                    else
                    {
                        audioBytes.Add(ConvertToWav(sound.ToArray()));
                    }
                }

                soundGroups.Add($"{sbfStruct.Name}_{soundbank.SoundbankDefinition.Name}", audioBytes);
            }
        }

        #endregion

        #region Write converted sounds to disk

        foreach(var soundGroup in soundGroups)
        {
            var soundbankName = soundGroup.Key.Split('_')[1];
            var dirPath = Path.Combine(outputPath, soundbankName);
            Directory.CreateDirectory(dirPath);
            
            var basename = soundGroup.Key;
            
            for (int i = 0; i < soundGroup.Value.Count; i++)
            {
                byte[] soundBytes = soundGroup.Value[i];
                
                string filename = basename;
                if (soundGroup.Value.Count > 1)
                    filename += $"_{i}";

                var filePath = Path.Combine(dirPath, $"{filename}.wav");

                File.WriteAllBytes(filePath, soundBytes);
            }
        }

        #endregion
    }
}
