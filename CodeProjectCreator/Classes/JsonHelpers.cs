using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CodeProjectCreator.Classes
{
    public static class JsonHelpers
    {
        public static async Task<T> ReadAndDeserializeJson<T>(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }

        // REPLACES contents of the given file with the given data
        public static async Task<T> WriteAndSerializeJson<T>(string filePath, T data)
        {
            using var writeStream = File.OpenWrite(filePath);

            await JsonSerializer.SerializeAsync<T>(writeStream, data);

            await writeStream.DisposeAsync();

            using var readStream = File.OpenRead(filePath);

            var result = await JsonSerializer.DeserializeAsync<T>(readStream);

            return result;
        }
    }
}
