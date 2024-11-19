using ManualHttp.Commands;

namespace Jwk;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var commander = new Commander().RegisterStaticMethodsOf<JwkCommands>();
            await commander.ExecuteAsync(args);
            return 0;
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
            return -1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return -1;
        }
    }
}
