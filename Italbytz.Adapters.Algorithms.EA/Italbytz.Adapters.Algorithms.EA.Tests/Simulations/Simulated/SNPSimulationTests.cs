using Italbytz.EA.Trainer;
using Italbytz.ML;
using Microsoft.ML;

namespace Italbytz.Adapters.Algorithms.EA.Tests.Simulations.Simulated;

[TestClass]
public class SNPSimulationTests
{
    [TestMethod]
    public void TestSimulation1()
    {
        for (var i = 1; i < 101; i++)
            GPASSimulation(1, i, AppDomain.CurrentDomain.BaseDirectory,
                new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
                    100, 100, 5, 1.1));
    }

    [TestMethod]
    public void TestSimulation2()
    {
        GPASSimulation(2, 1, AppDomain.CurrentDomain.BaseDirectory,
            new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
                100, 1000, 5, 1.1));
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
        // Just ensure the process completes
        Assert.IsNotNull(metrics);
    }
}