using System;
using System.Linq;

namespace ManualHttp.Commands
{
    public class CommandParameter
    {
        public string Name { get; }
        public int Position { get; }
        public Type Type { get; }
        public bool IsOptional { get; }
        public object DefaultValue { get; }

        public CommandParameter(string name, Type type, int position, bool isOptional, object defaultValue)
        {
            Name = name;
            Type = type;
            Position = position;
            IsOptional = isOptional;
            DefaultValue = defaultValue;
        }

        public object GetValue(Argument[] args)
        {
            var arg = args.FirstOrDefault(a => a.Matches(Position, Name));
            if (arg != null)
            {
                return Parse(arg);
            }
            if (IsOptional)
            {
                return DefaultValue;
            }
            throw new ArgumentException($"You must specify {Name}");
        }

        private object Parse(Argument argument)
        {
            switch (Type)
            {
                case Type t when t == typeof(string):
                    return argument.Value;
                case Type t when t == typeof(bool):
                    if (argument.Value == null)
                    {
                        return argument.IsNamed;
                    }
                    return bool.TryParse(argument.Value, out var ret) && ret;
                case Type t when t.IsEnum:
                    return Enum.Parse(Type, argument.Value, true);
                default:
                    return Convert.ChangeType(argument.Value, Type);
            }
        }

        public override string ToString()
        {
            return IsOptional ? $"[{Name}]" : Name;
        }
    }
}