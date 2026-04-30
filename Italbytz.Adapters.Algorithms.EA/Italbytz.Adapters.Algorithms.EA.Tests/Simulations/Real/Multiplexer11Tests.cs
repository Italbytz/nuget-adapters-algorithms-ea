using Italbytz.AI;
using Italbytz.EA.Fitness;
using Italbytz.EA.Trainer;
using Italbytz.AI;
using Italbytz.AI.ML.UciDatasets;
using Italbytz.AI.ML.Core.Configuration;

namespace Italbytz.Adapters.Algorithms.EA.Tests.Simulations.Real;

[TestClass]
public class Multiplexer11Tests : RealTests
{
    public Multiplexer11Tests()
    {
        Dataset = Data.Multiplexer11;
    }

    [TestMethod]
    public async Task TestLogicGp()
    {
        for (var i = 0; i < 10; i++)
        {
            ThreadSafeRandomNetCore.Seed = 42;
            var trainer =
                new LogicGpGpasBinaryTrainer(1000);
            var pipeline = Dataset.BuildPipeline(
                ThreadSafeMLContext.LocalMLContext, trainer,
                ScenarioType.Classification,
                ProcessingType.FeatureBinningAndCustomLabelMapping);
            var model = pipeline.Fit(Dataset.DataView);
            var predictions = model.Transform(Dataset.DataView);


            if (trainer is IInterpretableTrainer interpretableTrainer)
            {
                var finalModel = interpretableTrainer.Model;
                Console.WriteLine(finalModel);
            }
        }
        //Assert.IsTrue(metrics.MacroAccuracy > 0.38);
    }

    [TestMethod]
    public async Task SimulateLogicGpRlcw()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        ThreadSafeMLContext.Seed = 42;
        var trainer =
            new LogicGpRlcwMulticlassTrainer<BinaryClassificationOutput>(
                -100, 500, folds: 2,
                minMaxWeight: 1.0,
                usedMetric: (ClassMetric.Accuracy, Averaging.Macro));
        var pipeline = Dataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext, trainer,
            ScenarioType.Classification,
            ProcessingType.FeatureBinningAndCustomLabelMapping);
        var metrics = Simulate(pipeline, trainer, 0.2f, true);
    }
}