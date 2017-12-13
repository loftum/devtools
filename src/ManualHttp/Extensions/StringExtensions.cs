namespace ManualHttp.Extensions
{
    public static class StringExtensions
    {
        public static KeyValue ToKeyValue(this string value, char delimiter, bool trim = true)
        {
            if (value == null)
            {
                return null;
            }
            var index = value.IndexOf(delimiter);
            if (index < 0)
            {
                return new KeyValue(value.TrimIf(trim), null);
            }
            var key = value.Substring(0, index).TrimIf(trim);
            value = value.Substring(index + 1).TrimIf(trim);
            return new KeyValue(key, value);
        }

        public static string TrimIf(this string value, bool condition)
        {
            return condition ? value.Trim() : value;
        }
    }

    public class KeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public KeyValue()
        {
        }

        public KeyValue(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}