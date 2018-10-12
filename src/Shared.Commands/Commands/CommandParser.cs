using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Shared.Commands
{
    public class CommandParser<T>
    {
        private static readonly IDictionary<string, Command> Commands = new Dictionary<string, Command>();

        static CommandParser()
        {
            foreach (var method in typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                Commands[method.Name.ToLowerInvariant()] = new Command(method);
            }
        }

        public static Action Parse(string[] args)
        {
            if (!args.Any() || args[0].Equals("--help", StringComparison.InvariantCultureIgnoreCase))
            {
                return PrintUsage;
            }
            var name = args[0].ToLowerInvariant();
            if (Commands.TryGetValue(name, out var command))
            {
                return Execute(command, args.Skip(1));
            }
            return () => PrintUnknownCommand(args[0]);
        }

        private static Action Execute(Command command, IEnumerable<string> args)
        {
            return () =>
            {
                try
                {
                    command.Execute(args);
                }
                catch (TargetInvocationException e)
                {
                    if (e.InnerException != null)
                    {
                        throw e.InnerException;
                    }

                    throw;
                }
            };
        }

        private static void PrintUnknownCommand(string command)
        {
            Console.WriteLine($"Unknown command {command}");
            PrintUsage();
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} <command> [args]");
            Console.WriteLine("Args can be specified either in order or by name, i.e:");
            Console.WriteLine("  <command> value1 value2");
            Console.WriteLine("  <command> -arg1=value1 -arg2=value2");
            Console.WriteLine();
            foreach (var command in Commands.Values)
            {
                Console.WriteLine(command);
            }
        }
    }
}