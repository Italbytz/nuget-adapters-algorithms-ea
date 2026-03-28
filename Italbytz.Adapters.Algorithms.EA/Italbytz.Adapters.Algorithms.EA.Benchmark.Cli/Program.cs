using System;
using Italbytz.EA.Benchmark.Cli.Commands;

namespace Italbytz.EA.Benchmark.Cli;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return 1;
        }

        var command = args[0].ToLowerInvariant();
        var commandArgs = args.Length > 1 ? args[1..] : Array.Empty<string>();

        return command switch
        {
            "run" => BenchmarkCommand.Run(commandArgs),
            "compare" => CompareCommand.Run(commandArgs),
            "help" or "--help" or "-h" => ShowHelpAndReturn(),
            _ => UnknownCommand(command)
        };
    }

    private static int ShowHelpAndReturn()
    {
        PrintHelp();
        return 0;
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintHelp();
        return 1;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("LogicGP Benchmark CLI");
        Console.WriteLine("Usage:");
        Console.WriteLine("  run --trainer <name> --dataset <name> [--generations N] [--population N] [--seed N] [--output file]");
        Console.WriteLine("  compare --python <file> --csharp <file> [--output file]");
    }
}
