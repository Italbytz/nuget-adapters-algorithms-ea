using Italbytz.EA.Trainer;
using Italbytz.ML;
using Italbytz.ML.Data;
using Italbytz.ML.ModelBuilder.Configuration;

namespace Italbytz.Adapters.Algorithms.EA.Tests;

[TestClass]
public class LogicGpSpeedupTests
{
    private readonly IDataset _dataset;

    public LogicGpSpeedupTests()
    {
        _dataset = Data.NPHA;
    }

    [TestMethod]
    public void TestNewTrainer()
    {
        var trainer =
            new LogicGpRlcwMulticlassTrainer<TernaryClassificationOutput>(
                100, 100, 1.1);
        var pipeline = _dataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext, trainer,
            ScenarioType.Classification,
            ProcessingType.FeatureBinningAndCustomLabelMapping);
        var model = pipeline.Fit(_dataset.DataView);
        var predictions = model.Transform(_dataset.DataView);
        var metrics = ThreadSafeMLContext.LocalMLContext
            .MulticlassClassification
            .Evaluate(predictions);
    }
}