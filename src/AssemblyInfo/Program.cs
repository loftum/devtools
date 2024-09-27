using System.Reflection;
using System.Text;
using System.Text.Json;
using ManualHttp.Commands;

namespace AssemblyInfo;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var commander = new Commander().RegisterStaticMethodsOf<AssemblyInfoCommands>();
            await commander.ExecuteAsync(args);
            return 0;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e);
            return 1;
        }
    }
}

public class AssemblyInfoCommands
{
    public static void Version(string path)
    {
        var assembly = Assembly.LoadFrom(Translate(path));
        var version = assembly.GetName().Version?.ToString();
        Console.WriteLine(version ?? "null");
    }
    
    public static void Full(string path)
    {
        var assembly = Assembly.LoadFrom(Translate(path));

        var builder = new StringBuilder()
            .AppendLine($"FullName: '{assembly.FullName}'")
            .AppendLine($"Location: '{assembly.Location}'")
            .AppendLine($"Entrypoint: {Format(assembly.EntryPoint) ?? "none"}")
            .AppendLine($"Dynamic: {assembly.IsDynamic}");

        if (assembly.Modules.Any())
        {
            builder.AppendLine("Modules:");
            foreach (var module in assembly.Modules)
            {
                builder.AppendLine($"- {module.FullyQualifiedName}");
            }
        }
        
        Console.WriteLine(builder.ToString());
    }

    private static string Format(MethodInfo method)
    {
        if (method == null)
        {
            return null;
        }

        var builder = new StringBuilder($"{method.ReturnType} {method.Name}(");
        var parameters = method.GetParameters();
        if (parameters.Any())
        {
            builder.Append(string.Join(", ", parameters.Select(FormatParamter)));
        }

        builder.Append(")");
        return builder.ToString();
    }

    private static string FormatParamter(ParameterInfo parameter)
    {
        return $"{parameter.ParameterType} {parameter.Name}";
    }

    private static string Translate(string path)
    {
        if (path == null || !path.StartsWith("~"))
        {
            return path;
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, path.TrimStart('~', '/', '\\'));
    }
}