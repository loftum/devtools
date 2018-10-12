using System;
using System.Linq;
using System.Reflection;

namespace Shared.Commands
{
    public class CommandParameter
    {
        private readonly ParameterInfo _parameter;

        public CommandParameter(ParameterInfo parameter)
        {
            _parameter = parameter;
        }

        public object GetValue(Argument[] args)
        {
            var arg = args.FirstOrDefault(a => a.Matches(_parameter.Position, _parameter.Name));
            if (arg != null)
            {
                return Parse(arg);
            }
            if (_parameter.IsOptional)
            {
                return _parameter.DefaultValue;
            }
            throw new ArgumentException($"Please specify {_parameter.Name}");
        }

        private object Parse(Argument argument)
        {
            if (_parameter.ParameterType == typeof(string))
            {
                return argument.Value;
            }
            if (_parameter.ParameterType == typeof(bool))
            {
                if (argument.Value == null)
                {
                    return argument.IsNamed;
                }
                return bool.TryParse(argument.Value, out var ret) && ret;
            }
            return Convert.ChangeType(argument.Value, _parameter.ParameterType);
        }

        public override string ToString()
        {
            var p = $"{_parameter.ParameterType.Name} {_parameter.Name}";
            return _parameter.IsOptional ? $"[{p}]" : p;
        }
    }
}