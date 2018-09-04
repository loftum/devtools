using System;

namespace Http.Logging
{
    public static class Log
    {
        public static ILogger For(string name)
        {
            return new ConsoleLogger(name);
        }

        public static ILogger For(object owner)
        {
            return new ConsoleLogger(owner.GetType().Name);
        }

        public static ILogger For(Type type)
        {
            return For(type.Name);
        }

        public static ILogger For<T>()
        {
            return For(typeof(T));
        }
    }
}