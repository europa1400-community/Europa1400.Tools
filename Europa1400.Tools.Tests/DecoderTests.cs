using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Decoder;
using Xunit;

namespace Europa1400.Tools.Tests
{
    public class DecoderTests
    {
        [Fact]
        public void AssetDiscovererRegistry_HasAllRegistered()
        {
            Assert.NotNull(AssetDiscovererRegistry.Get<AgebAsset>());
            Assert.NotNull(AssetDiscovererRegistry.Get<AobjAsset>());
            Assert.NotNull(AssetDiscovererRegistry.Get<SbfAsset>());
            Assert.NotNull(AssetDiscovererRegistry.Get<GfxAsset>());
            Assert.NotNull(AssetDiscovererRegistry.Get<BgfAsset>());
            Assert.NotNull(AssetDiscovererRegistry.Get<TxsAsset>());
            Assert.NotNull(AssetDiscovererRegistry.Get<BafAsset>());
            Assert.NotNull(AssetDiscovererRegistry.Get<BafIniAsset>());
            Assert.NotNull(AssetDiscovererRegistry.Get<OgrAsset>());
            Assert.NotNull(AssetDiscovererRegistry.Get<Ed3Asset>());
            Assert.NotNull(AssetDiscovererRegistry.Get<ResAsset>());
            Assert.NotNull(AssetDiscovererRegistry.Get<FormAsset>());
            Assert.NotNull(AssetDiscovererRegistry.Get<ScrAsset>());
            Assert.NotNull(AssetDiscovererRegistry.Get<SavAsset>());
            Assert.NotNull(AssetDiscovererRegistry.Get<DummyAsset>());
        }

        private static byte[] ExtractBafFile(string entryName)
        {
            using var zip = System.IO.Compression.ZipFile.OpenRead(
                Path.Combine(EnvVariables.GameDirectoryPath, "Resources", "animations.bin"));
            var entry = zip.GetEntry(entryName);
            Assert.NotNull(entry);
            using var ms = new MemoryStream();
            entry.Open().CopyTo(ms);
            return ms.ToArray();
        }

        [Fact]
        public void BafDecoder_DecodesSmallFile()
        {
            var bytes = ExtractBafFile("_ADDON/AO_WIMPEL_ZIRKUSZELT.baf");
            using var ms = new MemoryStream(bytes);
            using var br = new BinaryReader(ms);
            var baf = Tools.Decoder.Baf.BafStruct.FromBytes(br);

            Assert.Equal("BGF", baf.Magic);
            Assert.Equal(8u, baf.BoneCount);
            Assert.NotEmpty(baf.Frames);
            Assert.Equal(8, baf.Frames[0].Positions.Length);
        }

        [Fact]
        public void BafDecoder_DecodesLargeFile()
        {
            var bytes = ExtractBafFile("BAUM/faellen_BUCHE_01.baf");
            using var ms = new MemoryStream(bytes);
            using var br = new BinaryReader(ms);
            var baf = Tools.Decoder.Baf.BafStruct.FromBytes(br);

            Assert.Equal("BGF", baf.Magic);
            Assert.Equal(4u, baf.BoneCount);
            Assert.NotEmpty(baf.Frames);
            Assert.Equal(4, baf.Frames[0].Positions.Length);
        }

        [Fact]
        public async Task OgrDecoder_DecodesSyntheticFile()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var name = "test_object";
                using (var fs = File.Create(tempFile))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(0x3A6C00BB); // magic
                    bw.Write(1u); // version
                    bw.Write((byte)name.Length);
                    bw.Write(System.Text.Encoding.ASCII.GetBytes(name));
                    bw.Write(new byte[] { 0x01, 0x02, 0x03, 0x04 }); // trailing data
                }

                var asset = new OgrAsset(tempFile, "test.ogr");
                var result = await new OgrDecoder().DecodeAsync(asset);
                var ogr = Assert.IsType<Tools.Decoder.Ogr.OgrStruct>(result);
                Assert.Equal("test_object", ogr.Name);
                Assert.NotEmpty(ogr.RawData);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task Ed3Decoder_DecodesSyntheticFile()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var fs = File.Create(tempFile))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write((byte)0xAF); // type
                    bw.Write((byte)0x00); // const
                    bw.Write((byte)0x6C);
                    bw.Write((byte)0x3A);
                    bw.Write(System.Text.Encoding.ASCII.GetBytes("MegaCam\0")); // name
                    for (int i = 0; i < 22; i++) bw.Write(0f); // 22 floats
                    bw.Write((byte)0); // num1
                    bw.Write((byte)0); // skip0 trailing zero
                }

                var asset = new Ed3Asset(tempFile, "test.ed3");
                var result = await new Ed3Decoder().DecodeAsync(asset);
                var ed3 = Assert.IsType<Tools.Decoder.Ed3.Ed3Struct>(result);
                Assert.Equal(0xAF, ed3.Type);
                Assert.NotNull(ed3.MainCameraElement);
                Assert.Equal("MegaCam", ed3.MainCameraElement.Name);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ResDecoder_DecodesSyntheticFile()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var entryCount = 3u;
                var nameOff = 48u; // absolute offset for name section
                var valueOff = 64u; // absolute offset for value section

                using (var fs = File.Create(tempFile))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(entryCount);
                    bw.Write(nameOff);
                    bw.Write(valueOff);
                    // offset table (3 entries at byte 12): 0, 10, 20
                    bw.Write(0u);
                    bw.Write(10u);
                    bw.Write(20u);
                    // pad to nameOff
                    bw.BaseStream.Position = nameOff;
                    bw.Write(System.Text.Encoding.Latin1.GetBytes("Name1\0"));
                    bw.Write(System.Text.Encoding.Latin1.GetBytes("Name2\0"));
                    bw.Write(System.Text.Encoding.Latin1.GetBytes("Name3\0"));
                    // pad to valueOff
                    bw.BaseStream.Position = valueOff;
                    bw.Write(System.Text.Encoding.Latin1.GetBytes("Value1\0"));
                    bw.Write(System.Text.Encoding.Latin1.GetBytes("Value2\0"));
                    bw.Write(System.Text.Encoding.Latin1.GetBytes("Value3\0"));
                }

                var asset = new ResAsset(tempFile, "test.res");
                var result = await new ResDecoder().DecodeAsync(asset);
                var res = Assert.IsType<Tools.Decoder.Res.ResStruct>(result);
                Assert.Equal(3u, res.EntryCount);
                Assert.Equal(3, res.NameStrings.Count);
                Assert.Equal(3, res.ValueStrings.Count);
                Assert.Contains("Name1", res.NameStrings);
                Assert.Contains("Value1", res.ValueStrings);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task FormDecoder_StillThrowsNotImplemented()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var asset = new FormAsset(tempFile, "test.form");
                await Assert.ThrowsAsync<NotImplementedException>(() => new FormDecoder().DecodeAsync(asset));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ScrDecoder_DecodesSyntheticFile()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var fs = File.Create(tempFile))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(new byte[] { 0x20, 0x10, 0x00, 0x18, 0x0C, 0x00, 0x30, 0x20, 0x08 });
                }

                var asset = new ScrAsset(tempFile, "test.scr");
                var result = await new ScrDecoder().DecodeAsync(asset);
                var scr = Assert.IsType<Tools.Decoder.Scr.ScrStruct>(result);
                Assert.Equal(3, scr.Triplets.Count);
                Assert.Equal(0x20, scr.Triplets[0].X);
                Assert.Equal(0x10, scr.Triplets[0].Y);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task SavDecoder_DecodesSyntheticGzipFile()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var originalData = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello"
                using (var fs = File.Create(tempFile))
                using (var gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionLevel.SmallestSize))
                {
                    gz.Write(originalData);
                }

                var asset = new SavAsset(tempFile, "test.sav");
                var result = await new SavDecoder().DecodeAsync(asset);
                var sav = Assert.IsType<Tools.Decoder.Sav.SavStruct>(result);
                Assert.True(sav.CompressedSize > 0);
                Assert.Equal(5, sav.DecompressedSize);
                Assert.Equal("Hello", System.Text.Encoding.ASCII.GetString(sav.DecompressedData));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
        [Fact]
        public async Task ResConverter_OutputsJson()
        {
            var res = new Tools.Decoder.Res.ResStruct
            {
                EntryCount = 2,
                Section1Offset = 48,
                Section2Offset = 64,
                NameStrings = new List<string> { "Key1", "Key2" },
                ValueStrings = new List<string> { "Val1", "Val2" },
                StringDataAreaBytes = Array.Empty<byte>()
            };
            var converter = new Tools.Pipeline.Converter.ResConverter();
            var progress = new PipelineProgress();
            var files = await converter.ConvertAsync(res, progress, null);
            var list = files.ToList();
            Assert.Single(list);
            Assert.Equal("strings.json", list[0].FilePath);
            var json = System.Text.Encoding.UTF8.GetString(list[0].Content);
            Assert.Contains("Key1", json);
            Assert.Contains("Val2", json);
        }

        [Fact]
        public async Task ScrConverter_OutputsJson()
        {
            var scr = new Tools.Decoder.Scr.ScrStruct
            {
                RawData = new byte[] { 0x20, 0x10, 0x00, 0x18, 0x0C, 0x00 },
                Triplets = new List<(byte X, byte Y, byte Z)>
                {
                    (0x20, 0x10, 0x00),
                    (0x18, 0x0C, 0x00)
                }
            };
            var converter = new Tools.Pipeline.Converter.ScrConverter();
            var progress = new PipelineProgress();
            var files = await converter.ConvertAsync(scr, progress, null);
            var list = files.ToList();
            Assert.Single(list);
            Assert.Equal("coordinates.json", list[0].FilePath);
            var json = System.Text.Encoding.UTF8.GetString(list[0].Content);
            Assert.Contains("32", json); // 0x20
            Assert.Contains("12", json); // 0x0C
        }

        [Fact]
        public async Task SavConverter_OutputsBytes()
        {
            var sav = new Tools.Decoder.Sav.SavStruct
            {
                CompressedSize = 100,
                DecompressedSize = 5,
                DecompressedData = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }
            };
            var converter = new Tools.Pipeline.Converter.SavConverter();
            var progress = new PipelineProgress();
            var files = await converter.ConvertAsync(sav, progress, null);
            var list = files.ToList();
            Assert.Equal(2, list.Count);
            var bytesFile = list.First(f => f.FilePath.EndsWith(".bytes"));
            Assert.Equal("savegame.bytes", bytesFile.FilePath);
            Assert.Equal("Hello", System.Text.Encoding.ASCII.GetString(bytesFile.Content));
            var metaFile = list.First(f => f.FilePath.EndsWith(".json"));
            Assert.Contains("100", System.Text.Encoding.UTF8.GetString(metaFile.Content));
        }

        [Fact]
        public async Task OgrConverter_OutputsJson()
        {
            var ogr = new Tools.Decoder.Ogr.OgrStruct
            {
                Name = "test_obj",
                RawData = new byte[] { 0x01, 0x02, 0x03 }
            };
            var converter = new Tools.Pipeline.Converter.OgrConverter();
            var progress = new PipelineProgress();
            var files = await converter.ConvertAsync(ogr, progress, null);
            var list = files.ToList();
            Assert.Single(list);
            Assert.Equal("test_obj.json", list[0].FilePath);
            var json = System.Text.Encoding.UTF8.GetString(list[0].Content);
            Assert.Contains("test_obj", json);
            Assert.Contains("RawDataBase64", json);
        }

        [Fact]
        public async Task Ed3Converter_OutputsSceneJson()
        {
            var ed3 = new Tools.Decoder.Ed3.Ed3Struct
            {
                Type = 0xAF,
                MainCameraElement = new Tools.Decoder.Ed3.MainCameraElement
                {
                    Name = "MegaCam",
                    Values = new List<float>(new float[22]),
                    Num1 = 0
                },
                SceneElements = new List<Tools.Decoder.Ed3.SceneElement>
                {
                    new Tools.Decoder.Ed3.SceneElement
                    {
                        OnesCount = 0,
                        Name = "test_element",
                        Width = 10,
                        Height = 5,
                        Type = 2,
                        DummyElement = new Tools.Decoder.Ed3.DummyElement
                        {
                            Flag1 = false,
                            Transform = new Tools.Decoder.Ed3.Transform
                            {
                                Position = new Tools.Decoder.Ed3.Vector3 { X = 1, Y = 2, Z = 3 },
                                Rotation = new Tools.Decoder.Ed3.Vector3 { X = 0, Y = 0, Z = 0 }
                            },
                            Flag2 = false,
                            Transforms = new List<Tools.Decoder.Ed3.Transform>()
                        },
                        SkipLength = 0,
                        ScriptElements = new List<Tools.Decoder.Ed3.ScriptElement>()
                    }
                }
            };
            var converter = new Tools.Pipeline.Converter.Ed3Converter();
            var progress = new PipelineProgress();
            var files = await converter.ConvertAsync(ed3, progress, null);
            var list = files.ToList();
            Assert.Single(list);
            Assert.Equal("scene.json", list[0].FilePath);
            var json = System.Text.Encoding.UTF8.GetString(list[0].Content);
            Assert.Contains("test_element", json);
            Assert.Contains("dummy_element", json);
            Assert.Contains("scene_elements", json);
        }
    }
}
