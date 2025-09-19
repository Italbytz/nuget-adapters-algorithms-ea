using Italbytz.AI;
using Italbytz.EA.Trainer;
using Italbytz.ML;
using Italbytz.ML.Data;
using Italbytz.ML.ModelBuilder.Configuration;

namespace Italbytz.Adapters.Algorithms.EA.Tests.Simulations.Real;

[TestClass]
public class NationalPollTests
{
    private readonly IDataset _dataset;

    public NationalPollTests()
    {
        _dataset = Data.NPHA;
    }

    [TestMethod]
    [TestCategory("FixedSeed")]
    public async Task TestLogicGp()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        var trainer =
            new LogicGpMulticlassTrainer<TernaryClassificationOutput>(10000);
        var pipeline = _dataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext, ScenarioType.Classification,
            trainer, true);
        var model = pipeline.Fit(_dataset.DataView);
        var predictions = model.Transform(_dataset.DataView);
        var metrics = ThreadSafeMLContext.LocalMLContext
            .MulticlassClassification
            .Evaluate(predictions);
        Assert.IsTrue(metrics.MacroAccuracy > 0.38);
    }
}