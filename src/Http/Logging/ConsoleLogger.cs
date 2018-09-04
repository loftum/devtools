using System;
using System.Collections.Generic;

namespace Http.Logging
{
    public class ConsoleLogger : ILogger
    {
        private static readonly Dictionary<LogLevel, ConsoleColor> Colors = new Dictionary<LogLevel, ConsoleColor>
        {
            [LogLevel.Trace] = ConsoleColor.Gray,
            [LogLevel.Debug] = ConsoleColor.Yellow,
            [LogLevel.Normal] = ConsoleColor.White,
            [LogLevel.Important] = ConsoleColor.Cyan
        };

        private readonly string _name;

        public ConsoleLogger(string name)
        {
            _name = name;
        }

        public void Log(LogLevel level, object message)
        {
            if (level < Settings.Instance.LogLevel)
            {
                return;
            }
            var lastColor = Console.ForegroundColor;
            Console.ForegroundColor = Colors[level];
            Console.WriteLine($"{_name}: {message}");
            Console.ForegroundColor = lastColor;
        }

        public void Trace(object message)
        {
            Log(LogLevel.Trace, message);
        }

        public void Debug(object message)
        {
            Log(LogLevel.Debug, message);
        }

        public void Normal(object message)
        {
            Log(LogLevel.Normal, message);
        }

        public void Important(object message)
        {
            Log(LogLevel.Important, message);
        }
    }
}