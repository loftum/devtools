using System.Net;

if (args.Length < 1)
{
    Console.WriteLine("Usage: urldecode <input>");
    return 0;
}

var input = File.Exists(args[0]) ? File.ReadAllText(args[0]) : args[0];

var decoded = WebUtility.UrlDecode(input);

Console.WriteLine(decoded);

return 0;