using System.Text;
using Europa1400.Tools.Structs.Baf;
using Europa1400.Tools.Pipeline;
using Europa1400.Tools.Pipeline.Converter;
using Europa1400.Tools.Structs.Sbf;

namespace Europa1400.Tools.Tests
{
    public class ConverterTests
    {
        private static readonly int[] ExpectedBafIniKeys = [1, 2, 3];

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
                File.WriteAllText(tempFile, "NumKeys=3\nKeys=1,2,3\nLoopIn=0\nLoopOut=10\n", Encoding.Latin1);
                var asset = new Pipeline.Assets.BafIniAsset(tempFile, "test.bafini");
                var decoder = new Pipeline.Decoder.BafIniDecoder();
                var result = await decoder.DecodeAsync(asset);

                var settings = Assert.IsType<BafIniSettings>(result);
                Assert.Equal(3, settings.NumKeys);
                Assert.Equal(ExpectedBafIniKeys, settings.Keys);
                Assert.Equal(0, settings.LoopIn);
                Assert.Equal(10, settings.LoopOut);
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
