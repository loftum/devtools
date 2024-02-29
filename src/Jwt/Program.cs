using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Jwt;

class Program
{
    private static readonly Dictionary<string, Func<string[], Task>> _commands = new()
    {
        ["decode"] = Decode,
        ["validate"] = Validate
    };

    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (!args.Any() || !_commands.TryGetValue(args[0], out var command))
            {
                PrintUsage();
                return 0;
            }

            await command(args);
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }
    
    private static async Task Validate(string[] args)
    {
        var handler = new JwtSecurityTokenHandler();
        var parameters = new TokenValidationParameters
        {
            
        };
        var result = await handler.ValidateTokenAsync(args[1], parameters);
        Console.WriteLine(Format(result));
    }

    private static string Format(TokenValidationResult result)
    {
        var builder = new StringBuilder();
        if (result.IsValid)
        {
            builder.AppendLine("Valid :)")
                .AppendLine($"Issuer: {result.Issuer}")
                .AppendLine($"TokenType: {result.TokenType}");
            builder.AppendLine(JsonSerializer.Serialize<object>(result.SecurityToken,
                new JsonSerializerOptions {WriteIndented = true}));
            // if (result.Claims.Any())
            // {
            //     builder.AppendLine("Claims:");
            //     builder.AppendLine(JsonSerializer.Serialize(result.Claims,
            //         new JsonSerializerOptions {WriteIndented = true}));
            // }
        }
        else
        {
            builder.AppendLine("Invalid :/");
            builder.AppendLine(result.Exception.Message);
        }

        return builder.ToString();
    }

    private static Task Decode(string[] arg)
    {
        return Task.CompletedTask;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Commands:");
        foreach (var command in _commands.Keys)
        {
            Console.WriteLine(command);
        }
    }
}