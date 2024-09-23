// Don't remove these:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManualHttp.Commands;

public class Command
{
    public string Name { get; }
    private readonly Func<object[], Task> _action;
    public IList<CommandParameter> Parameters { get; }

    public Command(string name, Func<object[], Task> action, IEnumerable<CommandParameter> parameters)
    {
        Name = name;
        _action = action;
        Parameters = parameters.ToList();
    }

    public Task ExecuteAsync(IEnumerable<string> args)
    {
        return _action(Parse(args));
    }

    private object[] Parse(IEnumerable<string> args)
    {
        var arguments = args.Select((a, ii) => new Argument(ii, a)).ToArray();
        if (arguments.Length > Parameters.Count)
        {
            throw new ArgumentException($"too many arguments. Usage: {this}");
        }
        Console.WriteLine($"{Name} {string.Join(" ", arguments.Cast<object>())}");
        return Parameters.Select(a => a.GetValue(arguments)).ToArray();
    }

    public override string ToString()
    {
        return $"{Name} {string.Join(" ", Parameters)}";
    }
}