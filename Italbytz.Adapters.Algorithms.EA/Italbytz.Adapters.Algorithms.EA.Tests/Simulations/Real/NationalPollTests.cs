using System.Globalization;
using Italbytz.AI;
using Italbytz.EA.Trainer;
using Italbytz.EA.Trainer.Gecco;
using Italbytz.ML;
using Italbytz.ML.Data;
using Italbytz.ML.ModelBuilder.Configuration;
using Microsoft.ML;
using Metrics = Italbytz.ML.Metrics;

namespace Italbytz.Adapters.Algorithms.EA.Tests.Simulations.Real;

[TestClass]
public class NationalPollTests
{
    private readonly IDataset _dataset;

    private readonly int[] _seeds =
    [
        42, 7, 13, 99, 256, 1024, 73, 3, 17, 23,
        5, 11, 19, 29, 31, 37, 41, 43, 47, 53,
        59, 61, 67, 71, 79, 83, 89, 97, 101, 103,
        107, 109, 113, 127, 131, 137, 139, 149, 151, 157,
        163, 167, 173, 179, 181, 191, 193, 197, 199, 211,
        223, 227, 229, 233, 239, 241, 251, 257, 263, 269,
        271, 277, 281, 283, 293, 307, 311, 313, 317, 331,
        337, 347, 349, 353, 359, 367, 373, 379, 383, 389,
        397, 401, 409, 419, 421, 431, 433, 439, 443, 449,
        457, 461, 463, 467, 479, 487, 491, 499, 503, 509
    ];

    private string _timeStamp;

    public NationalPollTests()
    {
        _dataset = Data.NPHA;
    }

    public StreamWriter SeedWriter { get; set; }

    public StreamWriter LogWriter { get; set; }

    [TestMethod]
    [TestCategory("FixedSeed")]
    public async Task TestLogicGp()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        var trainer =
            new LogicGpMulticlassTrainer<TernaryClassificationOutput>
            {
                RunStrategy = new RunStrategy(10000)
            };
        var pipeline = _dataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext, ScenarioType.Classification,
            trainer, true);
        var model = pipeline.Fit(_dataset.DataView);
        var predictions = model.Transform(_dataset.DataView);
        var metrics = ThreadSafeMLContext.LocalMLContext
            .MulticlassClassification
            .Evaluate(predictions);
        //Assert.IsTrue(metrics.MacroAccuracy > 0.38);
    }

    [TestMethod]
    public async Task SimulateLogicGp()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        ThreadSafeMLContext.Seed = 42;
        var trainer =
            new LogicGpMulticlassTrainer<TernaryClassificationOutput>
            {
                RunStrategy = new RunStrategy(10000)
            };
        var pipeline = _dataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext, ScenarioType.Classification,
            trainer, true);
        var metrics = Simulate(pipeline, 0.2f);
    }

    private IEnumerable<Metrics> Simulate(IEstimator<ITransformer> pipeline,
        float splitRatio, bool binary = false)
    {
        var metrics = new List<Metrics>();

        var tmpDir = Path.GetTempPath();
        _timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var logPath = Path.Combine(tmpDir,
            $"{_dataset.FilePrefix}_{_timeStamp}_Metrics.csv");
        LogWriter = new StreamWriter(logPath);
        LogWriter.WriteLine(
            "\"x\"");
        var seedPath = Path.Combine(tmpDir,
            $"{_dataset.FilePrefix}_{_timeStamp}_Seeds.csv");
        SeedWriter = new StreamWriter(seedPath);

        var files =
            _dataset.GetTrainValidateTestFiles(tmpDir,
                validateFraction: splitRatio, testFraction: splitRatio,
                seeds: _seeds);
        foreach (var file in files)
        {
            var test = 0;
            var trainDataview =
                _dataset.LoadFromTextFile(Path.Combine(tmpDir,
                    file.TrainValidateFileName), ',', true);
            var testDataview =
                _dataset.LoadFromTextFile(Path.Combine(tmpDir,
                    file.TestFileName), ',', true);
            var model = pipeline.Fit(trainDataview);
            var testResult = model.Transform(testDataview);
            var metric = ComputeMetrics(testResult,
                ThreadSafeMLContext.LocalMLContext, binary);
            metrics.Add(metric);
            var metricForCSV = metric.F1Score;
            LogWriter?.WriteLine(
                metricForCSV.ToString(CultureInfo.InvariantCulture));
            LogWriter?.Flush();
            SeedWriter?.WriteLine(file.TestFileName);
            SeedWriter?.Flush();
        }


        return metrics;
    }

    private Metrics? ComputeMetrics(IDataView testResult, MLContext mlContext,
        bool binary)
    {
        Metrics? metrics = null;
        try
        {
            var binaryMetrics = mlContext.BinaryClassification
                .Evaluate(testResult);
            metrics = new Metrics
            {
                IsBinaryClassification = true,
                Accuracy = binaryMetrics.Accuracy,
                AreaUnderRocCurve = binaryMetrics.AreaUnderRocCurve,
                F1Score = binaryMetrics.F1Score,
                AreaUnderPrecisionRecallCurve =
                    binaryMetrics.AreaUnderPrecisionRecallCurve
            };
        }
        catch (Exception e1)
        {
            try
            {
                var multiclassMetrics = mlContext.MulticlassClassification
                    .Evaluate(testResult);
                if (binary)
                    metrics = new Metrics
                    {
                        IsBinaryClassification = true,
                        Accuracy = multiclassMetrics.MicroAccuracy,
                        MacroAccuracy = multiclassMetrics.MacroAccuracy,
                        F1Score = multiclassMetrics.F1ScoreBinary()
                    };

                metrics = new Metrics
                {
                    IsMulticlassClassification = true,
                    Accuracy = multiclassMetrics.MicroAccuracy,
                    MacroAccuracy = multiclassMetrics.MacroAccuracy,
                    F1Score = multiclassMetrics.F1Macro()
                };
            }
            catch (Exception e2)
            {
                try
                {
                    var regressionMetrics = mlContext.Regression
                        .Evaluate(testResult);
                    metrics = new Metrics
                    {
                        IsRegression = true,
                        RSquared = regressionMetrics.RSquared,
                        MeanAbsoluteError =
                            regressionMetrics.MeanAbsoluteError,
                        MeanSquaredError =
                            regressionMetrics.MeanSquaredError,
                        RootMeanSquaredError =
                            regressionMetrics.RootMeanSquaredError
                    };
                }
                catch (Exception e3)
                {
                    Console.WriteLine(
                        "No binary, multiclass or regression metrics available.");
                }
            }
        }

        return metrics;
    }
}