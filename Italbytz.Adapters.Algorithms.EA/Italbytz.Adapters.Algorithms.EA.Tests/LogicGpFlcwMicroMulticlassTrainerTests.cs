using Italbytz.EA.Trainer;
using Italbytz.AI;
using Italbytz.AI.ML.UciDatasets;
using Italbytz.AI.ML.Core.Configuration;

namespace Italbytz.Adapters.Algorithms.EA.Tests;

[TestClass]
public class LogicGpFlcwMicroMulticlassTrainerTests
{
    private readonly IDataset _dataset;

    public LogicGpFlcwMicroMulticlassTrainerTests()
    {
        _dataset = Data.Iris;
    }

    [TestMethod]
    public async Task TestLogicGp()
    {
        var trainer =
            new LogicGpFlcwMicroMulticlassTrainer<TernaryClassificationOutput>(
                1000);
        var pipeline = _dataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext, trainer,
            ScenarioType.Classification,
            ProcessingType.FeatureBinningAndCustomLabelMapping);
        var model = pipeline.Fit(_dataset.DataView);
        var predictions = model.Transform(_dataset.DataView);
        var metrics = ThreadSafeMLContext.LocalMLContext
            .MulticlassClassification
            .Evaluate(predictions);
        Assert.IsTrue(metrics.MicroAccuracy > 0.6);
    }
}