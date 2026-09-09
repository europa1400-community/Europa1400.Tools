using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Decoder;
using Europa1400.Tools.Pipeline.Converter;
using Xunit;

namespace Europa1400.Tools.Tests
{
    public class Ed3RealFileTests
    {
        private static string GamePath => EnvVariables.GameDirectoryPath;

        [Fact]
        public async Task DecodeAndConvert_Geburt_Ed3()
        {
            var path = Path.Combine(GamePath, "Resources", "scenes_extracted", "Cutscenes", "Geburt.ed3");

            if (!File.Exists(path))
                return;

            var asset = new Ed3Asset(path, "Geburt.ed3");
            var decoder = new Ed3Decoder();
            var result = await decoder.DecodeAsync(asset);
            var ed3 = Assert.IsType<Structs.Ed3.Ed3Struct>(result);

            Assert.NotNull(ed3.MainCameraElement);
            Assert.Equal("MegaCam", ed3.MainCameraElement.Name);
            Assert.NotEmpty(ed3.SceneElements);

            var converter = new Ed3Converter();
            var progress = new PipelineProgress();
            var files = await converter.ConvertAsync(ed3, progress, null);
            var list = files.ToList();
            Assert.Single(list);
            var json = Encoding.UTF8.GetString(list[0].Content);
            Assert.Contains("SceneElements", json);
            Assert.Contains(ed3.SceneElements[0].Name, json);

            File.WriteAllText(Path.Combine(Path.GetTempPath(), "Geburt_scene.json"), json);
        }

        [Fact]
        public async Task DecodeAndConvert_Stadt_Nuernberg_Ed3()
        {
            var path = Path.Combine(GamePath, "Resources", "scenes_extracted", "AddOn", "Staedte", "stadt_NUERNBERG_0.ed3");

            if (!File.Exists(path))
                return;

            var asset = new Ed3Asset(path, "stadt_NUERNBERG_0.ed3");
            var decoder = new Ed3Decoder();
            var result = await decoder.DecodeAsync(asset);
            var ed3 = Assert.IsType<Structs.Ed3.Ed3Struct>(result);

            Assert.NotNull(ed3.MainCameraElement);
            Assert.NotEmpty(ed3.SceneElements);

            var cityElements = ed3.SceneElements.Where(s => s.CityElement != null).ToList();
            Assert.NotEmpty(cityElements);

            var converter = new Ed3Converter();
            var progress = new PipelineProgress();
            var files = await converter.ConvertAsync(ed3, progress, null);
            var list = files.ToList();
            var json = Encoding.UTF8.GetString(list[0].Content);
            Assert.Contains("CityElement", json);

            File.WriteAllText(Path.Combine(Path.GetTempPath(), "stadt_NUERNBERG_scene.json"), json);
        }
    }
}
