using System;
using System.Collections.Generic;
using System.Linq;

namespace Cert.Commands
{
    public class Command
    {
        public string Name { get; }
        private readonly Action<object[]> _action;
        public IList<CommandParameter> Parameters { get; set; }

        public Command(string name, Action<object[]> action, IEnumerable<CommandParameter> parameters)
        {
            Name = name;
            _action = action;
            Parameters = parameters.ToList();
        }

        public void Execute(IEnumerable<string> args)
        {
            _action(Parse(args));
        }

        private object[] Parse(IEnumerable<string> args)
        {
            var arguments = args.Select((a, ii) => new Argument(ii, a)).ToArray();
            if (arguments.Length > Parameters.Count)
            {
                throw new ArgumentException($"too many arguments. Usage: {this}");
            }
            Console.WriteLine($"{Name} {string.Join(" ", arguments.Cast<object>())}");
            return Parameters.Select(p => p.GetValue(arguments)).ToArray();
        }

        public override string ToString()
        {
            return $"{Name} {string.Join(" ", Parameters)}";
        }
    }
}