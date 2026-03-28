using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Trainer;
using Italbytz.ML;
using Italbytz.ML.Data;
using Italbytz.ML.ModelBuilder.Configuration;

namespace Italbytz.EA.Benchmark.Cli.Commands;

public static class BenchmarkCommand
{
    public class BenchmarkResult
    {
        [JsonPropertyName("trainer")]
        public string Trainer { get; set; } = string.Empty;
        
        [JsonPropertyName("dataset")]
        public string Dataset { get; set; } = string.Empty;
        
        [JsonPropertyName("f1_score")]
        public double F1Score { get; set; }

        [JsonPropertyName("f1_averaging")]
        public string F1Averaging { get; set; } = "macro";
        
        [JsonPropertyName("train_time_ms")]
        public long TrainTimeMs { get; set; }

        [JsonPropertyName("total_time_ms")]
        public long TotalTimeMs { get; set; }

        [JsonPropertyName("num_rules")]
        public int NumRules { get; set; }

        [JsonPropertyName("num_atoms")]
        public int NumAtoms { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = "ok";
        
        [JsonPropertyName("error")]
        public string? Error { get; set; }
        
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    public static int Run(string[] args)
    {
        var options = ParseArgs(args);
        var trainerType = GetOption(options, "trainer", "flcw-macro") ?? "flcw-macro";
        var datasetName = GetOption(options, "dataset", "iris") ?? "iris";
        var generations = GetIntOption(options, "generations", 100);
        var population = GetIntOption(options, "population", 1000);
        var seed = GetIntOption(options, "seed", 42);
        var f1Averaging = (GetOption(options, "f1-averaging", "macro") ?? "macro").ToLowerInvariant();
        var outputPath = GetOption(options, "output", null);

        var exitCode = RunBenchmark(trainerType, datasetName, generations, population, seed, f1Averaging, outputPath);
        return exitCode;
    }

    private static int RunBenchmark(
        string trainerType,
        string datasetName,
        int generations,
        int population,
        int seed,
        string f1Averaging,
        string? outputPath)
    {
        try
        {
            var totalStopwatch = Stopwatch.StartNew();

            var dataset = ResolveDataset(datasetName);
            var trainStopwatch = Stopwatch.StartNew();

            var (metrics, trainer, fittedModel) = TrainAndEvaluate(trainerType, dataset, generations, population, seed);

            trainStopwatch.Stop();
            totalStopwatch.Stop();

            var f1Score = ComputeF1(metrics.ConfusionMatrix.Counts, f1Averaging);
            var resolvedModel = ResolveModel(trainer, fittedModel);
            var (numRules, numAtoms) = ExtractModelSize(resolvedModel);

            var result = new BenchmarkResult
            {
                Trainer = trainerType,
                Dataset = datasetName,
                F1Score = f1Score,
                F1Averaging = f1Averaging,
                TrainTimeMs = trainStopwatch.ElapsedMilliseconds,
                TotalTimeMs = totalStopwatch.ElapsedMilliseconds,
                NumRules = numRules,
                NumAtoms = numAtoms,
                Status = "ok",
                Timestamp = DateTime.UtcNow
            };
            
            var jsonOptions = new JsonSerializerOptions 
            { 
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(result, jsonOptions);

            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                File.WriteAllText(outputPath!, json);
            }
            else
            {
                Console.WriteLine(json);
            }

            return 0;
        }
        catch (Exception ex)
        {
            var errorResult = new BenchmarkResult
            {
                Trainer = trainerType,
                Dataset = datasetName,
                F1Averaging = f1Averaging,
                Status = "error",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            };
            
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            Console.Error.WriteLine(JsonSerializer.Serialize(errorResult, jsonOptions));
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

    private static int GetIntOption(Dictionary<string, string> options, string key, int defaultValue)
    {
        if (!options.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        return int.TryParse(value, out var parsed) ? parsed : defaultValue;
    }

    private static object ResolveDataset(string datasetName)
    {
        var normalized = NormalizeName(datasetName);
        var dataType = typeof(Data);

        var matchingProperty = dataType
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .FirstOrDefault(p => NormalizeName(p.Name) == normalized);

        if (matchingProperty == null)
        {
            var available = string.Join(", ", dataType
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Select(p => p.Name)
                .OrderBy(n => n));
            throw new ArgumentException($"Unknown dataset '{datasetName}'. Available datasets: {available}");
        }

        var value = matchingProperty.GetValue(null);
        if (value == null)
        {
            throw new InvalidOperationException($"Dataset property '{matchingProperty.Name}' is null.");
        }

        return value;
    }

    private static (Microsoft.ML.Data.MulticlassClassificationMetrics metrics, LogicGpTrainer<TernaryClassificationOutput> trainer, object fittedModel)
        TrainAndEvaluate(string trainerType, object dataset, int generations, int population, int seed)
    {
        var trainer = CreateTrainer(trainerType, generations, population, seed);

        dynamic dynDataset = dataset;

        var pipeline = dynDataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext,
            trainer,
            ScenarioType.Classification,
            ProcessingType.FeatureBinningAndCustomLabelMapping);

        var model = pipeline.Fit(dynDataset.DataView);
        var predictions = model.Transform(dynDataset.DataView);
        var metrics = ThreadSafeMLContext.LocalMLContext.MulticlassClassification.Evaluate(predictions);

        return (metrics, trainer, model);
    }

    private static LogicGpTrainer<TernaryClassificationOutput> CreateTrainer(
        string trainerType,
        int generations,
        int population,
        int seed)
    {
        _ = seed;

        return trainerType.ToLowerInvariant() switch
        {
            "flcw-macro" => new LogicGpFlcwMacroMulticlassTrainer<TernaryClassificationOutput>(generations),
            "flcw-micro" => new LogicGpFlcwMicroMulticlassTrainer<TernaryClassificationOutput>(generations),
            "rlcw-macro" => new LogicGpRlcwMulticlassTrainer<TernaryClassificationOutput>(
                phase1Time: generations,
                phase2Time: generations,
                maxIndividuals: population,
                usedMetric: (ClassMetric.F1, Averaging.Macro)),
            "rlcw-micro" => new LogicGpRlcwMulticlassTrainer<TernaryClassificationOutput>(
                phase1Time: generations,
                phase2Time: generations,
                maxIndividuals: population,
                usedMetric: (ClassMetric.F1, Averaging.Micro)),
            _ => throw new ArgumentException($"Unknown trainer '{trainerType}'. Supported: flcw-macro, flcw-micro, rlcw-macro, rlcw-micro")
        };
    }

    private static string NormalizeName(string value)
    {
        return new string(value
            .ToLowerInvariant()
            .Where(ch => char.IsLetterOrDigit(ch))
            .ToArray());
    }

    private static double ComputeF1(IReadOnlyList<IReadOnlyList<double>> confusion, string averaging)
    {
        return averaging switch
        {
            "micro" => ComputeMicroF1(confusion),
            _ => ComputeMacroF1(confusion)
        };
    }

    private static double ComputeMacroF1(IReadOnlyList<IReadOnlyList<double>> confusion)
    {
        if (confusion.Count == 0)
        {
            return 0;
        }

        var classCount = confusion.Count;
        var f1Sum = 0.0;

        for (var i = 0; i < classCount; i++)
        {
            var tp = confusion[i][i];
            var fn = confusion[i].Sum() - tp;

            var fp = 0.0;
            for (var r = 0; r < classCount; r++)
            {
                fp += confusion[r][i];
            }

            fp -= tp;

            var precision = tp + fp > 0 ? tp / (tp + fp) : 0.0;
            var recall = tp + fn > 0 ? tp / (tp + fn) : 0.0;
            var f1 = precision + recall > 0 ? 2 * precision * recall / (precision + recall) : 0.0;
            f1Sum += f1;
        }

        return f1Sum / classCount;
    }

    private static double ComputeMicroF1(IReadOnlyList<IReadOnlyList<double>> confusion)
    {
        if (confusion.Count == 0)
        {
            return 0;
        }

        var total = 0.0;
        var tp = 0.0;

        for (var r = 0; r < confusion.Count; r++)
        {
            for (var c = 0; c < confusion[r].Count; c++)
            {
                total += confusion[r][c];
                if (r == c)
                {
                    tp += confusion[r][c];
                }
            }
        }

        if (total <= 0)
        {
            return 0;
        }

        // For single-label multiclass, micro-F1 equals accuracy.
        return tp / total;
    }

    private static (int numRules, int numAtoms) ExtractModelSize(IIndividual? model)
    {
        if (model?.Genotype == null)
        {
            return (0, 0);
        }

        var genotype = model.Genotype;
        var polynomialProperty = genotype.GetType().GetProperty("Polynomial");
        if (polynomialProperty == null)
        {
            return ExtractFromGenotypeSize(genotype);
        }

        var polynomial = polynomialProperty.GetValue(genotype);
        if (polynomial == null)
        {
            return ExtractFromGenotypeSize(genotype);
        }

        var monomialsProperty = polynomial.GetType().GetProperty("Monomials");
        if (monomialsProperty?.GetValue(polynomial) is not IEnumerable monomials)
        {
            return ExtractFromGenotypeSize(genotype);
        }

        var ruleCount = 0;
        var atomCount = 0;

        foreach (var monomial in monomials)
        {
            ruleCount++;
            var literalsProperty = monomial?.GetType().GetProperty("Literals");
            if (literalsProperty?.GetValue(monomial) is IEnumerable literals)
            {
                foreach (var _ in literals)
                {
                    atomCount++;
                }
            }
        }

        if (ruleCount == 0 && atomCount == 0)
        {
            return ExtractFromGenotypeSize(genotype);
        }

        return (ruleCount, atomCount);
    }

    private static (int numRules, int numAtoms) ExtractFromGenotypeSize(object genotype)
    {
        var sizeProperty = genotype.GetType().GetProperty("Size");
        if (sizeProperty?.GetValue(genotype) is int size && size > 0)
        {
            return (1, size);
        }

        return (0, 0);
    }

    private static IIndividual? ResolveModel(LogicGpTrainer<TernaryClassificationOutput> trainer, object fittedModel)
    {
        if (trainer.Model != null)
        {
            return trainer.Model;
        }

        if (trainer.FinalPopulation is IEnumerable population)
        {
            var best = SelectBestIndividual(population);
            if (best != null)
            {
                return best;
            }
        }

        var fromTransformer = FindModelInObjectGraph(fittedModel, 3);
        return fromTransformer;
    }

    private static IIndividual? SelectBestIndividual(IEnumerable population)
    {
        IIndividual? best = null;

        foreach (var candidate in population)
        {
            if (candidate is not IIndividual current)
            {
                continue;
            }

            if (best == null)
            {
                best = current;
                continue;
            }

            if (IsBetter(current, best))
            {
                best = current;
            }
        }

        return best;
    }

    private static bool IsBetter(IIndividual candidate, IIndividual incumbent)
    {
        var candidateFitness = candidate.LatestKnownFitness;
        var incumbentFitness = incumbent.LatestKnownFitness;

        if (candidateFitness is IComparable candidateComparable && incumbentFitness != null)
        {
            try
            {
                if (candidateComparable.CompareTo(incumbentFitness) > 0)
                {
                    return true;
                }
            }
            catch
            {
                // Ignore compare failures and fall back to size.
            }
        }

        return candidate.Size > incumbent.Size;
    }

    private static IIndividual? FindModelInObjectGraph(object? root, int maxDepth)
    {
        if (root == null || maxDepth < 0)
        {
            return null;
        }

        if (root is IIndividual individual)
        {
            return individual;
        }

        var type = root.GetType();

        var modelProperty = type.GetProperty("Model", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (modelProperty?.GetValue(root) is IIndividual modelFromProperty)
        {
            return modelFromProperty;
        }

        foreach (var property in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            object? value;
            try
            {
                value = property.GetValue(root);
            }
            catch
            {
                continue;
            }

            if (ReferenceEquals(value, null))
            {
                continue;
            }

            if (value is IIndividual found)
            {
                return found;
            }

            if (value is string)
            {
                continue;
            }

            var nested = FindModelInObjectGraph(value, maxDepth - 1);
            if (nested != null)
            {
                return nested;
            }
        }

        foreach (var field in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
        {
            var value = field.GetValue(root);
            if (value is IIndividual found)
            {
                return found;
            }

            if (value == null || value is string)
            {
                continue;
            }

            var nested = FindModelInObjectGraph(value, maxDepth - 1);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }
}
