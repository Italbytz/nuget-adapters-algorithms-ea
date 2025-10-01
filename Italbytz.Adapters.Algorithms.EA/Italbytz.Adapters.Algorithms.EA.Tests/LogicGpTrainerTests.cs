using Italbytz.EA.Trainer;
using Italbytz.ML;
using Italbytz.ML.Data;
using Italbytz.ML.ModelBuilder.Configuration;

namespace Italbytz.Adapters.Algorithms.EA.Tests;

[TestClass]
public class LogicGpTrainerTests
{
    private readonly IDataset _dataset;

    public LogicGpTrainerTests()
    {
        _dataset = Data.Iris;
    }

    [TestMethod]
    public async Task TestLogicGp()
    {
        var trainer =
            new LogicGpFlcwMacroMulticlassTrainer<TernaryClassificationOutput>(
                10000);
        var pipeline = _dataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext, trainer,
            ScenarioType.Classification,
            ProcessingType.FeatureBinningAndCustomLabelMapping);
        var model = pipeline.Fit(_dataset.DataView);
        var predictions = model.Transform(_dataset.DataView);
        var metrics = ThreadSafeMLContext.LocalMLContext
            .MulticlassClassification
            .Evaluate(predictions);
        Assert.IsTrue(metrics.MacroAccuracy > 0.6);
    }
}