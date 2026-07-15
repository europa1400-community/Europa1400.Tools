using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Europa1400.Tools.Pipeline.Output
{
    public static class ObjectSerializationInputLoader
    {
        /// <summary>
        ///     Loads a JSON-serialized object from a file path and deserializes it with type information.
        /// </summary>
        public static async Task<object> LoadAsync(string path, CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(path);
            using var jsonReader = new JsonTextReader(reader);

            var serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            var result = await Task.Run(() => serializer.Deserialize(jsonReader), cancellationToken);
            return result!;
        }

        /// <summary>
        ///     Generic overload that attempts to cast the loaded object to the expected type.
        ///     May throw if type mismatches.
        /// </summary>
        public static async Task<T> LoadAsync<T>(string path, CancellationToken cancellationToken = default)
        {
            var obj = await LoadAsync(path, cancellationToken);
            return (T)obj;
        }
    }
}