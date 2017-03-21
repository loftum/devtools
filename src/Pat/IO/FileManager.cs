using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace Pat.IO
{
    public class FileManager
    {
        private const string Folder = "Files";
        private static readonly string BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Folder);

        static FileManager()
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
        }

        public void SaveJson(object item)
        {
            if (item == null)
            {
                return;
            }
            var path = GetPathFor(item.GetType());
            File.WriteAllText(path, JsonConvert.SerializeObject(item, Formatting.Indented));
        }

        public T LoadJson<T>()
        {
            var path = GetPathFor<T>();
            return File.Exists(path)
                ? JsonConvert.DeserializeObject<T>(File.ReadAllText(path))
                : default(T);
        }

        public T LoadJsonOrDefault<T>(T defaultValue = default(T))
        {
            try
            {
                return LoadJson<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        public void SaveBinary<T>(T item)
        {
            var file = new FileInfo(GetPathFor<T>());
            using (var stream = file.Exists ? file.OpenWrite() : file.Create())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, item);
            }
        }

        public T LoadBinary<T>()
        {
            var file = new FileInfo(GetPathFor<T>());
            if (!file.Exists)
            {
                return default(T);
            }
            using (var stream = file.OpenRead())
            {
                var formatter = new BinaryFormatter();
                return (T) formatter.Deserialize(stream);
            }
        }

        public T LoadBinaryOrDefault<T>(T defaultValue = default(T))
        {
            try
            {
                return LoadBinary<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        private static string GetPathFor<T>()
        {
            return GetPathFor(typeof(T));
        }

        private static string GetPathFor(Type type)
        {
            var filename = $"{type.Name}.json";
            var path = Path.Combine(BasePath, filename);
            return path;
        }
    }
}