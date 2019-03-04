using System;
using ManualHttp.Commands;

namespace Http
{
    class Program
    {
        static int Main(string[] args)
        {
            var commander = new Commander().RegisterStaticMethodsOf<HttpCommands>();
            try
            {

                commander.Execute(args);
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
}
