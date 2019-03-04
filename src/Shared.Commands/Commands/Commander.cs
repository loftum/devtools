using System;
using System.Collections.Generic;
using System.Linq;

namespace ManualHttp.Commands
{
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
        
        public void Execute(string[] args)
        {
            var action = GetAction(args);
            action?.Invoke();
        }

        public Action GetAction(string[] args)
        {
            if (!args.Any() || args[0].Equals("--help", StringComparison.InvariantCultureIgnoreCase))
            {
                return PrintUsage;
            }
            var name = args[0].ToLowerInvariant();
            if (Commands.TryGetValue(name, out var command))
            {
                return () => command.Execute(args.Skip(1));
            }
            return () => PrintUnknownCommand(args[0]);
        }

        private void PrintUnknownCommand(string command)
        {
            Console.WriteLine($"Unknown command {command}");
            PrintUsage();
        }

        private void PrintUsage()
        {
            Console.WriteLine("Usage:");
            foreach (var command in Commands.Values)
            {
                Console.WriteLine(command);
            }
        }
    }
}
