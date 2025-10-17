using Italbytz.AI;
using Italbytz.EA.Fitness;
using Italbytz.EA.Trainer;
using Italbytz.ML;
using Italbytz.ML.Data;
using Italbytz.ML.ModelBuilder.Configuration;

namespace Italbytz.Adapters.Algorithms.EA.Tests.Simulations.Real;

[TestClass]
public class HeartDiseaseBinaryTests : RealTests
{
    public HeartDiseaseBinaryTests()
    {
        Dataset = Data.HeartDiseaseBinary;
    }

    [TestMethod]
    public async Task TestLogicGp()
    {
        for (var i = 0; i < 100; i++)
        {
            ThreadSafeRandomNetCore.Seed = 42;
            var trainer =
                new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
                    10, 2, folds: 5,
                    minMaxWeight: 1.05,
                    usedMetric: (ClassMetric.F1, Averaging.Macro));
            var pipeline = Dataset.BuildPipeline(
                ThreadSafeMLContext.LocalMLContext, trainer,
                ScenarioType.Classification,
                ProcessingType.FeatureBinningAndCustomLabelMapping);
            var model = pipeline.Fit(Dataset.DataView);
            var predictions = model.Transform(Dataset.DataView);


            if (trainer is IInterpretableTrainer interpretableTrainer)
            {
                var finalModel = interpretableTrainer.Model;
                //Console.WriteLine(finalModel);
            }
        }
        //Assert.IsTrue(metrics.MacroAccuracy > 0.38);
    }

    [TestMethod]
    public async Task SimulateLogicGpFlcw()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        ThreadSafeMLContext.Seed = 42;
        var trainer =
            new LogicGpFlcwMacroMulticlassTrainer<
                BinaryClassificationOutput>(10000);
        var pipeline = Dataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext, trainer,
            ScenarioType.Classification,
            ProcessingType.FeatureBinningAndCustomLabelMapping);
        var metrics = Simulate(pipeline, 0.2f);
    }

    [TestMethod]
    public async Task SimulateLogicGpRlcw()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        ThreadSafeMLContext.Seed = 42;
        var trainer =
            new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
                -3, 100, folds: 5,
                minMaxWeight: 1.05,
                usedMetric: (ClassMetric.F1, Averaging.Macro));
        var pipeline = Dataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext, trainer,
            ScenarioType.Classification,
            ProcessingType.FeatureBinningAndCustomLabelMapping);
        var metrics = Simulate(pipeline, 0.2f);
    }
}