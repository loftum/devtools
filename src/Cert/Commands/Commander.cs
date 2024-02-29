using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Cert.Commands
{
    public class Commander
    {
        public IDictionary<string, Command> Commands { get; }

        public Commander()
        {
            Commands = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase);
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
    
    public static class CommandBuilder
    {
        public static Command FromMethod(MethodInfo method)
        {
            var parameters = method.GetParameters().Select(ParameterBuilder.FromParameter);
            return new Command(method.Name, arguments => method.Invoke(null, arguments), parameters);
        }
    }

    public static class ParameterBuilder
    {
        public static CommandParameter FromParameter(ParameterInfo parameter)
        {
            return new CommandParameter(parameter.Name, parameter.ParameterType, parameter.Position, parameter.IsOptional, parameter.DefaultValue);
        }
    }

    public static class CommanderExtensions
    {
        public static Commander RegisterExpression<TDelegate>(this Commander commander, Expression<TDelegate> expression)
        {
            return commander.RegisterExpression((LambdaExpression)expression);
        }

        public static Commander RegisterExpression(this Commander commander, LambdaExpression lambda)
        {
            return commander;
        }

        public static Commander RegisterStaticMethodsOf<T>(this Commander commander)
        {
            return commander.RegisterStaticMethodsOf(typeof(T));
        }

        public static Commander RegisterStaticMethodsOf(this Commander commander, Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                commander.RegisterMethod(method);
            }
            return commander;
        }

        public static Commander RegisterMethod(this Commander commander, MethodInfo method)
        {
            commander.Register(CommandBuilder.FromMethod(method));
            return commander;
        }
    }
}