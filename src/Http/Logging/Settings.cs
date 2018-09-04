using System;
using System.IO;

namespace Http.Logging
{
    public class Settings
    {
        private static readonly string BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");
        private static readonly string FilePath = Path.Combine(BasePath, "settings.json");
        private LogLevel _logLevel;
        public static Settings Instance { get; }

        public LogLevel LogLevel
        {
            get => _logLevel;
            set
            {
                _logLevel = value;
                Save();
            }
        }

        static Settings()
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
            Instance = Load();
        }

        private static Settings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    return Json.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new Settings();
                }
                return new Settings();
            }
            catch
            {
                return new Settings();
            }
        }

        public void Save()
        {
            File.WriteAllText(FilePath, Json.Serialize(this));
        }
    }
}