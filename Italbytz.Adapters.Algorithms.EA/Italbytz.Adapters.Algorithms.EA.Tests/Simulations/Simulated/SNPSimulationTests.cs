using System.Diagnostics;
using Italbytz.AI;
using Italbytz.EA.Trainer;
using Italbytz.ML;
using Microsoft.ML;
using LogicGpGraph = Italbytz.EA.Trainer.Gecco.LogicGpGraph;

namespace Italbytz.Adapters.Algorithms.EA.Tests.Simulations.Simulated;

[TestClass]
public class SNPSimulationTests
{
    [TestMethod]
    public void Benchmark1()
    {
        var stopwatch = new Stopwatch();
        var trainer1 =
            new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
                1, 2, folds: 5, minMaxWeight: 1.1);
        stopwatch.Reset();
        stopwatch.Start();
        GPASSimulation(1, 1, AppDomain.CurrentDomain.BaseDirectory, trainer1);
        stopwatch.Stop();
        Console.WriteLine($"{stopwatch.ElapsedMilliseconds}");
    }

    [TestMethod]
    public void Benchmark()
    {
        var stopwatch = new Stopwatch();
        var trainer1 =
            new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
                10000, 10000, folds: 5, minMaxWeight: 1.1);
        var trainer2 =
            new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
                1000, 1000, folds: 5, minMaxWeight: 1.1,
                algorithmGraph: new LogicGpGraph(10000, 20, 10));
        //new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
        //  10, 100, 5, 1.1, new LogicGpGraph());
        stopwatch.Reset();
        stopwatch.Start();
        GPASSimulation(1, 1, AppDomain.CurrentDomain.BaseDirectory, trainer1);
        stopwatch.Stop();
        Console.WriteLine($"{stopwatch.ElapsedMilliseconds}");
        stopwatch.Reset();
        stopwatch.Start();
        GPASSimulation(1, 1, AppDomain.CurrentDomain.BaseDirectory, trainer2);
        stopwatch.Stop();
        Console.WriteLine($"{stopwatch.ElapsedMilliseconds}");
        stopwatch.Reset();
        stopwatch.Start();
        GPASSimulation(1, 1, AppDomain.CurrentDomain.BaseDirectory, trainer1);
        stopwatch.Stop();
        Console.WriteLine($"{stopwatch.ElapsedMilliseconds}");
        stopwatch.Reset();
        stopwatch.Start();
        GPASSimulation(1, 1, AppDomain.CurrentDomain.BaseDirectory, trainer2);
        stopwatch.Stop();
        Console.WriteLine($"{stopwatch.ElapsedMilliseconds}");
    }

    [TestMethod]
    public void TestSimulation1()
    {
        GPASSimulation(1, 1, AppDomain.CurrentDomain.BaseDirectory,
            new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
                5, 5, folds: 5, minMaxWeight: 1.1));
        return;
        for (var i = 1; i < 101; i++)
            GPASSimulation(1, i, AppDomain.CurrentDomain.BaseDirectory,
                new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
                    10, 10, folds: 5, minMaxWeight: 1.1));
    }

    [TestMethod]
    public void TestSimulation2()
    {
        GPASSimulation(2, 1, AppDomain.CurrentDomain.BaseDirectory,
            new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
                100, 1000, folds: 5, minMaxWeight: 1.1));
    }

    [TestMethod]
    public void TestSimulation3()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        ThreadSafeMLContext.Seed = 42;
        GPASSimulation(3, 1, AppDomain.CurrentDomain.BaseDirectory,
            new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
                5, 5, folds: 5, minMaxWeight: 1.1));
    }

    private void GPASSimulation(int simulation, int dataset,
        string baseDirectory, IEstimator<ITransformer> trainer)
    {
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var data = SNPHelper.LoadData(baseDirectory, simulation, dataset);
        var pipeline = SNPHelper.BuildPipeline(mlContext, trainer);
        var model = pipeline.Fit(data);
        var predictions = model.Transform(data);
        var metrics = mlContext.BinaryClassification.Evaluate(predictions);
        if (trainer is IInterpretableTrainer interpretableTrainer)
        {
            var logicGpModel = interpretableTrainer.Model;
            if (logicGpModel != null)
                Console.WriteLine(logicGpModel);
        }

        // Just ensure the process completes
        Assert.IsNotNull(metrics);
    }
}