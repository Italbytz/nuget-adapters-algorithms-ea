using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Italbytz.EA.Benchmark.Cli.Commands;

public static class CompareCommand
{
    public static int Run(string[] args)
    {
        var options = ParseArgs(args);
        var pythonPath = GetOption(options, "python", null);
        var csharpPath = GetOption(options, "csharp", null);
        var outputPath = GetOption(options, "output", null);

        if (string.IsNullOrWhiteSpace(pythonPath) || string.IsNullOrWhiteSpace(csharpPath))
        {
            Console.Error.WriteLine("Missing required options: --python and --csharp");
            return 1;
        }

        return CompareResults(pythonPath!, csharpPath!, outputPath);
    }

    private static int CompareResults(string pythonFile, string csharpFile, string? outputPath)
    {
        try
        {
            var pythonJson = File.ReadAllText(pythonFile);
            var csharpJson = File.ReadAllText(csharpFile);
            
            var pythonResult = JsonSerializer.Deserialize<Dictionary<string, object>>(pythonJson);
            var csharpResult = JsonSerializer.Deserialize<Dictionary<string, object>>(csharpJson);
            
            var comparison = new
            {
                python = pythonResult,
                csharp = csharpResult,
                comparison = new
                {
                    f1_diff = ExtractDouble(pythonResult, "f1_score") - ExtractDouble(csharpResult, "f1_score"),
                    time_ratio = ExtractDouble(csharpResult, "total_time_ms") / ExtractDouble(pythonResult, "total_time_ms"),
                    timestamp = DateTime.UtcNow
                }
            };
            
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(comparison, jsonOptions);
            
            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                File.WriteAllText(outputPath!, json);
                Console.WriteLine($"Comparison saved to {outputPath}");
            }
            else
            {
                Console.WriteLine(json);
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error comparing results: {ex.Message}");
            return 1;
        }
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Length; i++)
        {
            var current = args[i];
            if (!current.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            var key = current[2..];
            var value = i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal)
                ? args[++i]
                : "true";
            result[key] = value;
        }

        return result;
    }

    private static string? GetOption(Dictionary<string, string> options, string key, string? defaultValue)
    {
        return options.TryGetValue(key, out var value) ? value : defaultValue;
    }
    
    private static double ExtractDouble(Dictionary<string, object>? dict, string key)
    {
        if (dict == null || !dict.TryGetValue(key, out var value))
            return 0;

        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.String when double.TryParse(element.GetString(), out var parsed) => parsed,
                _ => 0
            };
        }

        return Convert.ToDouble(value);
    }
}
