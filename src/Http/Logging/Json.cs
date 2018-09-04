using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Http.Logging
{
    public static class Json
    {
        public static readonly JsonSerializerSettings DefaultSettings;

        static Json()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter());
            DefaultSettings = settings;
            JsonConvert.DefaultSettings = () => DefaultSettings;
        }

        public static string Serialize(object o)
        {
            return JsonConvert.SerializeObject(o, Formatting.Indented);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}