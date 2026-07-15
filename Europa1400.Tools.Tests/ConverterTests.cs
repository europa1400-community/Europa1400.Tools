using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Converter;
using Europa1400.Tools.Pipeline;
using Europa1400.Tools.Pipeline.Converter;
using Europa1400.Tools.Pipeline.Output;
using Europa1400.Tools.Structs;
using Europa1400.Tools.Structs.Bgf;
using Europa1400.Tools.Structs.Sbf;
using Xunit;

namespace Europa1400.Tools.Tests
{
    public class ConverterTests
    {
        [Fact]
        public async Task SbfRawConverter_OutputsCorrectExtensions_ForWav()
        {
            var sbf = CreateMinimalSbfStruct(SoundType.Wav);

            var converter = new SbfRawConverter();
            var files = await converter.ConvertAsync(sbf, new PipelineProgress(), null, CancellationToken.None);

            var list = files.ToList();
            Assert.Single(list);
            Assert.EndsWith(".wav", list[0].FilePath);
        }

        [Fact]
        public async Task SbfRawConverter_OutputsCorrectExtensions_ForMp3()
        {
            var sbf = CreateMinimalSbfStruct(SoundType.Mp3);

            var converter = new SbfRawConverter();
            var files = await converter.ConvertAsync(sbf, new PipelineProgress(), null, CancellationToken.None);

            var list = files.ToList();
            Assert.Single(list);
            Assert.EndsWith(".mp3", list[0].FilePath);
        }

        [Fact]
        public async Task BafIniDecoder_ReadsTextContent()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "key=value", Encoding.Latin1);
                var asset = new Pipeline.Assets.BafIniAsset(tempFile, "test.bafini");
                var decoder = new Pipeline.Decoder.BafIniDecoder();
                var result = await decoder.DecodeAsync(asset);

                Assert.IsType<string>(result);
                Assert.Equal("key=value", result);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task TxsDecoder_ThrowsOnInvalidData()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFile, new byte[] { 0, 0, 0, 0 });
                var asset = new Pipeline.Assets.TxsAsset(tempFile, "test.txs");
                var decoder = new Pipeline.Decoder.TxsDecoder();

                var ex = await Record.ExceptionAsync(() => decoder.DecodeAsync(asset));
                Assert.NotNull(ex);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        private static SbfStruct CreateMinimalSbfStruct(SoundType soundType)
        {
            var def = new SoundbankDefinitionStruct
            {
                Address = 0,
                Name = "bank1",
                Padding = new byte[8],
                SoundbankType = SoundbankType.Single
            };

            return new SbfStruct
            {
                Name = "test",
                SoundbankCount = 1,
                MagicBytes = new byte[4],
                Padding = new byte[8],
                SoundbankDefinitions = new[] { def },
                Soundbanks = new[]
                {
                    new SoundbankStruct
                    {
                        SoundbankDefinition = def,
                        SoundCount = 1,
                        SoundbankHeader = null,
                        SoundDefinitions = new[]
                        {
                            new SoundDefinitionStruct
                            {
                                SoundType = soundType,
                                Length = 4,
                                Magic = 0
                            }
                        },
                        Sounds = new[]
                        {
                            new byte[] { 0x01, 0x02, 0x03, 0x04 }
                        }
                    }
                }
            };
        }
    }
}
