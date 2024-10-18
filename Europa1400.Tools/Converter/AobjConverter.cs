using Europa1400.Tools.Decoder.Aobj;
using Europa1400.Tools.Interfaces;
using System.Text.Json;

namespace Europa1400.Tools.Converter
{
    internal class AobjConverter : IConverter
    {
        public void Convert(string pathToGameFiles, string targetDirectory)
        {
            var aobjFile = new FileInfo(Path.Combine(pathToGameFiles, "Data\\A_Obj.dat"));
            var objectsJson = new FileInfo(Path.Combine(targetDirectory, "objects.json"));

            using var binaryReader = new BinaryReader(File.OpenRead(aobjFile.FullName));
            using var outputStream = new FileStream(objectsJson.FullName, FileMode.Create);
            var aobjStruct = AobjStruct.FromBytes(binaryReader);

            var jsonWriterOptions = new JsonWriterOptions
            {
                Indented = true
            };

            using var jsonWriter = new Utf8JsonWriter(outputStream, jsonWriterOptions);

            jsonWriter.WriteStartObject();
            jsonWriter.WriteStartArray("Objects");

            foreach (var aobject in aobjStruct.Objects)
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WriteNumber("Type", aobject.Type);
                jsonWriter.WriteString("Name", aobject.Name);
                jsonWriter.WriteNumber("Level", aobject.Level);
                jsonWriter.WriteNumber("Time", aobject.Time);
                jsonWriter.WriteStartArray("UnknownData1");
                foreach (var data in aobject.UnknownData1)
                {
                    jsonWriter.WriteNumberValue(data);
                }
                jsonWriter.WriteEndArray();
                jsonWriter.WriteStartArray("UnknownData2");
                foreach (var data in aobject.UnknownData2)
                {
                    jsonWriter.WriteNumberValue(data);
                }
                jsonWriter.WriteEndArray();
                jsonWriter.WriteNumber("Unknown1", aobject.Unknown1);
                jsonWriter.WriteNumber("Price", aobject.Price);
                jsonWriter.WriteNumber("Unknown2", aobject.Unknown2);
                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
            jsonWriter.WriteEndObject();
        }
    }
}
