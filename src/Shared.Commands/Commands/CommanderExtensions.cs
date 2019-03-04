using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ManualHttp.Commands
{
    public static class CommandBuilder
    {
        public static Command FromMethod(MethodInfo method)
        {
            var parameters = method.GetParameters().Select(ParameterBuilder.FromParameter);
            return new Command(method.Name.ToLowerInvariant(), arguments => method.Invoke(null, arguments), parameters);
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