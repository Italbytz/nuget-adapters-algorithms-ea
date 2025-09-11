using Italbytz.AI;
using Italbytz.EA.Control;
using Italbytz.EA.Fitness;
using Italbytz.EA.Graph.Common;
using Italbytz.EA.Initialization;
using Italbytz.EA.Searchspace;
using GenerationStoppingCriterion =
    Italbytz.EA.StoppingCriterion.GenerationStoppingCriterion;

namespace Italbytz.Adapters.Algorithms.EA.Tests;

[TestClass]
public class LogicGpTests
{
    private readonly double[][] _features =
        Enumerable.Range(0, 63)
            .Select(i => new[] { 0.1 * i })
            .ToArray();

    private readonly double[] _labels = Enumerable.Range(0, 63)
        .Select(i => Math.Sin(0.1 * i)).ToArray();

    [TestMethod]
    public async Task TestLogicGp()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        var logicGp = new EvolutionaryAlgorithm
        {
            FitnessFunction = new AbsoluteDeviation(_features, _labels),
            SearchSpace = new TinyGpSearchSpace
            {
                MaxLen = 10000,
                Depth = 5,
                VariableCount = 1,
                NumberConst = 100,
                MinRandom = -5,
                MaxRandom = 5
            },
            AlgorithmGraph = new LogicGPGeccoGraph()
        };
        logicGp.Initialization = new RandomInitialization(logicGp)
        {
            Size = 10000
        };

        logicGp.StoppingCriteria =
        [
            new GenerationStoppingCriterion(logicGp)
            {
                Limit = 100000
            }
        ];
        await logicGp.Run();
    }
}