using Europa1400.Tools.Decoder.Ageb;
using Europa1400.Tools.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Europa1400.Tools.Converter
{
    internal class AgebConverter : IConverter
    {
        public void Convert(string pathToGameFiles, string targetDirectory)
        {
            var agebFile = new FileInfo(Path.Combine(pathToGameFiles, "Data\\A_Geb.dat"));
            var buildingsJson = new FileInfo(Path.Combine(targetDirectory, "buildings.json"));

            using var binaryReader = new BinaryReader(File.OpenRead(agebFile.FullName));
            using var outputStream = new FileStream(buildingsJson.FullName, FileMode.Create);
            var agebStruct = AgebStruct.FromBytes(binaryReader);

            var jsonWriterOptions = new JsonWriterOptions
            {
                Indented = true
            };

            using var jsonWriter = new Utf8JsonWriter(outputStream, jsonWriterOptions);

            jsonWriter.WriteStartObject();
            jsonWriter.WriteStartArray("Buildings");
            
            foreach (var building in agebStruct.Buildings)
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WriteNumber("GroupId", building.GroupId);
                jsonWriter.WriteString("Name", building.Name);
                jsonWriter.WriteNumber("Unknown1", building.Unknown1);
                jsonWriter.WriteStartArray("UnkownData1");
                foreach (var data in building.UnkownData1)
                {
                    jsonWriter.WriteNumberValue(data);
                }
                jsonWriter.WriteEndArray();
                jsonWriter.WriteStartArray("UnknownData2");
                foreach (var data in building.UnknownData2)
                {
                    jsonWriter.WriteNumberValue(data);
                }
                jsonWriter.WriteEndArray();
                jsonWriter.WriteStartArray("UnknownData3");
                foreach (var data in building.UnknownData3)
                {
                    jsonWriter.WriteNumberValue(data);
                }
                jsonWriter.WriteEndArray();
                jsonWriter.WriteStartArray("UnknownData4");
                foreach (var data in building.UnknownData4)
                {
                    jsonWriter.WriteNumberValue(data);
                }
                jsonWriter.WriteEndArray();
                jsonWriter.WriteStartArray("UnknownData5");
                foreach (var data in building.UnknownData5)
                {
                    jsonWriter.WriteNumberValue(data);
                }
                jsonWriter.WriteEndArray();
                jsonWriter.WriteStartObject("Coordinates1");
                jsonWriter.WriteNumber("X", building.Coordinates1.X);
                jsonWriter.WriteNumber("Y", building.Coordinates1.Y);
                jsonWriter.WriteEndObject();
                jsonWriter.WriteStartObject("Coordinates2");
                jsonWriter.WriteNumber("X", building.Coordinates2.X);
                jsonWriter.WriteNumber("Y", building.Coordinates2.Y);
                jsonWriter.WriteEndObject();
                jsonWriter.WriteNumber("Time", building.Time);
                jsonWriter.WriteNumber("Level", building.Level);
                jsonWriter.WriteNumber("Unknown2", building.Unknown2);
                jsonWriter.WriteNumber("Price", building.Price);
                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
            jsonWriter.WriteEndObject();
        }
    }
}
