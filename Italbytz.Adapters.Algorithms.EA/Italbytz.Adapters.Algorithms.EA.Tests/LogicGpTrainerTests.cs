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
    public void TestSave()
    {
        var trainer =
            new LogicGpFlcwMacroMulticlassTrainer<TernaryClassificationOutput>(
                10);
        var pipeline = _dataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext, trainer,
            ScenarioType.Classification,
            ProcessingType.FeatureBinningAndCustomLabelMapping);
        pipeline.Fit(_dataset.DataView);
        if (trainer is ISaveable saveable)
        {
            var stream = new MemoryStream();
            saveable.Save(stream);
            stream.Position = 0;
            using var reader = new StreamReader(stream, leaveOpen: true);
            var content = reader.ReadToEnd();
            Console.WriteLine(content);
        }
    }


    [TestMethod]
    public void TestLoad()
    {
        var trainer =
            new LogicGpFlcwMacroMulticlassTrainer<TernaryClassificationOutput>(
                10);
        var pipeline = _dataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext, trainer,
            ScenarioType.Classification,
            ProcessingType.FeatureBinningAndCustomLabelMapping);
        pipeline.Fit(_dataset.DataView);
        var tmpFolder = Path.GetTempPath();
        var modelPath = Path.Combine(tmpFolder, "logicgp_model.json");
        if (trainer is ISaveable saveable)
            using (var fileStream = new FileStream(modelPath, FileMode.Create,
                       FileAccess.Write))
            {
                saveable.Save(fileStream);
            }

        var loadStream = new FileStream(modelPath, FileMode.Open,
            FileAccess.Read);
        var loadedTrainer =
            LogicGpTrainer<TernaryClassificationOutput>
                .Load(loadStream);
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