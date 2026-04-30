using Italbytz.AI;
using Italbytz.EA.Fitness;
using Italbytz.EA.Trainer;
using Italbytz.AI;
using Italbytz.AI.ML.UciDatasets;
using Italbytz.AI.ML.Core.Configuration;

namespace Italbytz.Adapters.Algorithms.EA.Tests.Simulations.Real;

[TestClass]
public class CarTests : RealTests
{
    public CarTests()
    {
        Dataset = Data.CarEvaluation;
    }

    [TestMethod]
    public async Task SimulateLogicGpRlcw()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        ThreadSafeMLContext.Seed = 42;
        var trainer =
            new LogicGpRlcwMulticlassTrainer<QuaternaryClassificationOutput>(
                20, 100, folds: 5,
                minMaxWeight: 1.05,
                usedMetric: (ClassMetric.F1, Averaging.Macro));
        var pipeline = Dataset.BuildPipeline(
            ThreadSafeMLContext.LocalMLContext, trainer,
            ScenarioType.Classification,
            ProcessingType.FeatureBinningAndCustomLabelMapping);
        var metrics = Simulate(pipeline, trainer, 0.2f);
    }
}