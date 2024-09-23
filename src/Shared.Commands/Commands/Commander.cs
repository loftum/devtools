using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManualHttp.Commands;

public class Commander
{
    public IDictionary<string, Command> Commands { get; }

    public Commander()
    {
        Commands = new Dictionary<string, Command>();
    }

    public void Register(Command command)
    {
        Commands[command.Name] = command;
    }
        
    public Task ExecuteAsync(string[] args)
    {
        var action = GetAction(args);
        return action == null ? Task.CompletedTask : action.Invoke();
    }

    public Func<Task> GetAction(string[] args)
    {
        if (!args.Any() || args[0].Equals("--help", StringComparison.InvariantCultureIgnoreCase))
        {
            return PrintUsage;
        }
        var name = args[0].ToLowerInvariant();
        if (Commands.TryGetValue(name, out var command))
        {
            return () => command.ExecuteAsync(args.Skip(1));
        }
        return () => PrintUnknownCommand(args[0]);
    }

    private Task PrintUnknownCommand(string command)
    {
        Console.WriteLine($"Unknown command {command}");
        return PrintUsage();
    }

    private Task PrintUsage()
    {
        Console.WriteLine("Usage:");
        foreach (var command in Commands.Values)
        {
            Console.WriteLine(command);
        }

        return Task.CompletedTask;
    }
}