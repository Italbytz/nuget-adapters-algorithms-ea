using Italbytz.EA.Trainer;
using Italbytz.ML;
using Italbytz.ML.Data;
using Italbytz.ML.ModelBuilder.Configuration;

namespace Italbytz.Adapters.Algorithms.EA.Tests;

[TestClass]
public class LogicGpGpasBinaryTrainerTest
{
    private readonly IDataset _dataset;

    public LogicGpGpasBinaryTrainerTest()
    {
        _dataset = Data.HeartDiseaseBinary;
    }

    [TestMethod]
    public async Task TestLogicGp()
    {
        var trainer =
            new LogicGpGpasBinaryTrainer(1000);
        var pipeline = _dataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext, trainer,
            ScenarioType.Classification,
            ProcessingType.FeatureBinningAndCustomLabelMapping);
        var model = pipeline.Fit(_dataset.DataView);
        var predictions = model.Transform(_dataset.DataView);
        var metrics = ThreadSafeMLContext.LocalMLContext
            .BinaryClassification
            .Evaluate(predictions);
        Assert.IsTrue(metrics.Accuracy > 0.6);
    }
}