using System.Text;
using ManualHttp.Commands;

namespace Base64;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var commander = new Commander().RegisterStaticMethodsOf<Base64Commands>();
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

public class Base64Commands
{
    public static void Encode(string data)
    {
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        Console.WriteLine(base64);
    }
    
    public static void UrlEncode(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        var base64 = new StringBuilder(Convert.ToBase64String(bytes))
            .Replace("=", string.Empty)
            .Replace('+', '-')
            .Replace('/', '_')
            .ToString();
        
        Console.WriteLine(base64);
    }

    public static void Decode(string base64)
    {
        var decoded = Convert.FromBase64String(base64);
        var utf8 = Encoding.UTF8.GetString(decoded);
        Console.WriteLine(utf8);
    }
    
    public static void UrlDecode(string base64url)
    {
        var padLength = (4 - base64url.Length % 4) % 4;
        var base64 = new StringBuilder(base64url)
            .Replace('-', '+')
            .Replace('_', '/')
            .Append('=', padLength)
            .ToString();
        Decode(base64);
    }
}

