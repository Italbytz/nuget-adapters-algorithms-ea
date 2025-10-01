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
        GPASSimulation(1, AppDomain.CurrentDomain.BaseDirectory,
            new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
                1000, 1000, 1.1));
    }

    [TestMethod]
    public void TestSimulation2()
    {
        GPASSimulation(2, AppDomain.CurrentDomain.BaseDirectory,
            new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
                100, 1000, 1.1));
    }


    private void GPASSimulation(int simulation,
        string baseDirectory, IEstimator<ITransformer> trainer)
    {
        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var data = SNPHelper.LoadData(baseDirectory, simulation, 1);
        var pipeline = SNPHelper.BuildPipeline(mlContext, trainer);
        var model = pipeline.Fit(data);
        var predictions = model.Transform(data);
        var metrics = mlContext.BinaryClassification.Evaluate(predictions);
        // Just ensure the process completes
        Assert.IsNotNull(metrics);
    }
}