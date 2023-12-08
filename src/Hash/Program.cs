using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Hash;

class Program
{
    private static readonly Dictionary<string, Func<HashAlgorithm>> Algorithms =
        new(StringComparer.InvariantCultureIgnoreCase)
        {
            ["sha1"] = SHA1.Create,
            ["sha256"] = SHA256.Create,
            ["sha384"] = SHA384.Create,
            ["sha512"] = SHA512.Create,
            ["md5"] = MD5.Create
        };
    
    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return 0;
            }
            
            var algorithmName = args[0];

            if (!Algorithms.TryGetValue(algorithmName, out var algorithmFactory))
            {
                Console.WriteLine($"Invalid algorithm '{algorithmName}'");
                return 1;
            }

            await using var stream = GetInputAsync(args);
            using var algorithm = algorithmFactory();
            var hash = await algorithm.ComputeHashAsync(stream);
            Console.WriteLine(Convert.ToBase64String(hash));
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }

    private static Stream GetInputAsync(string[] args)
    {
        if (args[1] != "-f")
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(args[1]));
        }
        if (args.Length < 3)
        {
            throw new ArgumentException("Filename missing");
        }

        var filename = args[2];
        if (!File.Exists(filename))
        {
            throw new ArgumentException($"Cannot find file '{filename}'");
        }

        return File.OpenRead(filename);

    }

    private static void PrintUsage()
    {
        var builder = new StringBuilder()
            .AppendLine("Computes hash of input")
            .AppendLine($"{Process.GetCurrentProcess().ProcessName} <alg> [-f(ile)] <input>")
            .AppendLine("Input is either file or commandline input")
            .AppendLine()
            .AppendLine("Supported algorithms:");

        foreach (var alg in Algorithms.Keys)
        {
            builder.AppendLine(alg);
        }
        
        Console.WriteLine(builder);
    }
}