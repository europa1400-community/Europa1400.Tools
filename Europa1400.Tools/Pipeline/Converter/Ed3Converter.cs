using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Decoder.Ed3;
using Europa1400.Tools.Pipeline.Output;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Europa1400.Tools.Pipeline.Converter
{
    public class Ed3Converter : IConverter
    {
        public Task<IEnumerable<IFileExport>> ConvertAsync(object input, PipelineProgress pipelineProgress,
            IProgress<PipelineProgress>? progress, CancellationToken cancellationToken = default)
        {
            if (input is not Ed3Struct ed3)
                throw new ArgumentException("Input is not of type Ed3Struct");

            var sceneElements = new JArray();
            foreach (var se in ed3.SceneElements)
                sceneElements.Add(ConvertSceneElement(se));

            var root = new JObject
            {
                ["scene_elements"] = sceneElements
            };

            var json = root.ToString(Formatting.Indented);

            var files = new List<IFileExport>
            {
                new FileExport
                {
                    FilePath = "scene.json",
                    Content = System.Text.Encoding.UTF8.GetBytes(json)
                }
            };

            pipelineProgress.Current += 1;
            progress?.Report(pipelineProgress);

            return Task.FromResult(files.AsEnumerable());
        }

        private static JObject ConvertSceneElement(SceneElement se)
        {
            var obj = new JObject
            {
                ["ones_count"] = se.OnesCount,
                ["name"] = se.Name ?? "",
                ["width"] = se.Width,
                ["height"] = se.Height ?? 0,
                ["type"] = se.Type,
                ["skip_length"] = se.SkipLength
            };

            if (se.DummyElement != null)
                obj["dummy_element"] = ConvertTransformElement(se.DummyElement);
            else
                obj["dummy_element"] = null;

            if (se.ObjectElement != null)
                obj["object_element"] = ConvertObjectElement(se.ObjectElement);
            else
                obj["object_element"] = null;

            if (se.CityElement != null)
                obj["city_element"] = ConvertCityElement(se.CityElement);
            else
                obj["city_element"] = null;

            return obj;
        }

        private static JObject ConvertTransformElement(Transform transform)
        {
            return new JObject
            {
                ["position"] = ConvertVector3(transform.Position),
                ["rotation"] = ConvertVector3(transform.Rotation)
            };
        }

        private static JObject ConvertTransformElement(Europa1400.Tools.Decoder.Ed3.DummyElement dummy)
        {
            var obj = new JObject
            {
                ["transform"] = ConvertTransformElement(dummy.Transform),
                ["transforms"] = new JArray(dummy.Transforms.Select(ConvertTransformElement))
            };
            return obj;
        }

        private static JObject ConvertObjectElement(ObjectElement objEl)
        {
            var obj = new JObject
            {
                ["transform"] = ConvertTransformElement(objEl.Transform),
                ["transforms"] = new JArray(objEl.Transforms.Select(ConvertTransformElement))
            };
            if (objEl.Name != null)
                obj["name"] = objEl.Name;
            return obj;
        }

        private static JObject ConvertCityElement(CityElement city)
        {
            var obj = new JObject
            {
                ["height_data1"] = new JArray(city.HeightData1),
                ["has_water_element"] = city.HasWaterElement
            };

            if (city.CityElementNum1.HasValue)
                obj["num1"] = city.CityElementNum1.Value;
            if (city.CityElementNum2.HasValue)
                obj["num2"] = city.CityElementNum2.Value;
            if (city.Flag1.HasValue)
                obj["flag1"] = city.Flag1.Value;
            if (city.HeightData2 != null)
                obj["height_data2"] = new JArray(city.HeightData2);

            return obj;
        }

        private static JObject ConvertVector3(Vector3 v)
        {
            return new JObject
            {
                ["x"] = v.X,
                ["y"] = v.Y,
                ["z"] = v.Z
            };
        }
    }
}
