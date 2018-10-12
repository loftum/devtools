using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Shared.Commands
{
    public class Command
    {
        private readonly string _description;
        private readonly MethodInfo _method;
        private readonly IList<CommandParameter> _parameters;

        public Command(MethodInfo method)
        {
            _method = method;
            _description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;
            _parameters = method.GetParameters().Select(p => new CommandParameter(p)).ToList();
        }

        public void Execute(IEnumerable<string> args)
        {
            _method.Invoke(null, Parse(args));
        }

        private object[] Parse(IEnumerable<string> args)
        {
            var arguments = args.Select((a, ii) => new Argument(ii, a)).ToArray();
            if (arguments.Length > _parameters.Count)
            {
                throw new ArgumentException($"too many arguments. Usage: {this}");
            }
            return _parameters.Select(a => a.GetValue(arguments)).ToArray();
        }

        public override string ToString()
        {
            var builder = new StringBuilder().AppendLine($"{_method.Name.ToLowerInvariant()} {string.Join(" ", _parameters)}");
            if (_description != null)
            {
                builder.AppendLine($"   - {_description}");
            }
            return builder.ToString();
        }
    }
}