using System;
using System.Security.Authentication;
using ManualHttp.Commands;

namespace ManualHttp;

class Program
{
    static int Main(string[] args)
    {
        var commander = new Commander().RegisterStaticMethodsOf<ManualHttpCommands>();
        try
        {
            commander.ExecuteAsync(args);
            return 0;
        }
        catch (AuthenticationException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Could not authenticate");
            throw;
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