using System;
using System.Linq;

namespace ManualHttp.Commands;

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

    private static bool IsNullable(Type type, out Type inner)
    {
        if (type.IsGenericType)
        {
            var def = type.GetGenericTypeDefinition();
            if (def == typeof(Nullable<>))
            {
                inner = type.GenericTypeArguments[0];
                return true;
            }
        }

        inner = default;
        return false;
    }
    
    private object Parse(Argument argument)
    {
        var type = IsNullable(Type, out var inner) ? inner : Type;
        
        switch (type)
        {
            case not null when type == typeof(string):
                return argument.Value;
            case not null when type == typeof(bool):
                if (argument.Value == null)
                {
                    return argument.IsNamed;
                }
                return bool.TryParse(argument.Value, out var ret) && ret;
            case { IsEnum: true}:
                return Enum.Parse(type, argument.Value, true);
            case not null when type == typeof(DateTimeOffset):
                return DateTimeOffset.Parse(argument.Value);
            case not null when type == typeof(DateTime):
                return DateTime.Parse(argument.Value);
            default:
                return Convert.ChangeType(argument.Value, type);
        }
    }

    public override string ToString()
    {
        return IsOptional ? $"[{Name}]" : Name;
    }
}