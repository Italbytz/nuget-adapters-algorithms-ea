using Italbytz.EA.Trainer;
using Italbytz.ML;
using Microsoft.ML;

namespace Italbytz.Adapters.Algorithms.EA.Tests.Simulations.Simulated;

[TestClass]
public class SNPSimulationTests
{
    [TestMethod]
    public void TestSNP()
    {
        GPASSimulation(1, AppDomain.CurrentDomain.BaseDirectory,
            new LogicGpGpasBinaryTrainer(10000));
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