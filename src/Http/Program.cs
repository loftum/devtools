using System;
using System.Threading.Tasks;
using ManualHttp.Commands;

namespace Http;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var commander = new Commander().RegisterStaticMethodsOf<HttpCommands>();
        try
        {
            await commander.ExecuteAsync(args);
            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Console.ResetColor();
            return -1;
        }
    }
}