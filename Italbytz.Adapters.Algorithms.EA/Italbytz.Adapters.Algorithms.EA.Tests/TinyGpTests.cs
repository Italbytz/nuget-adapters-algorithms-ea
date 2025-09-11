using Italbytz.AI;
using Italbytz.EA.Control;
using Italbytz.EA.Fitness;
using Italbytz.EA.Graph.Common;
using Italbytz.EA.Initialization;
using Italbytz.EA.Searchspace;
using Italbytz.EA.StoppingCriterion;
using GenerationStoppingCriterion =
    Italbytz.EA.StoppingCriterion.GenerationStoppingCriterion;

namespace Italbytz.Adapters.Algorithms.EA.Tests;

[TestClass]
public class TinyGpTests
{
    private readonly float[][] _features =
        Enumerable.Range(0, 63)
            .Select(i => new[] { 0.1f * i })
            .ToArray();

    private readonly float[] _labels = Enumerable.Range(0, 63)
        .Select(i => (float)Math.Sin(0.1 * i)).ToArray();

    [TestMethod]
    public async Task TestTinyGp()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        var tinyGp = new EvolutionaryAlgorithm
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
            AlgorithmGraph = new TinyGPGraph()
        };
        tinyGp.Initialization = new RandomInitialization(tinyGp)
        {
            Size = 10000
        };

        tinyGp.StoppingCriteria =
        [
            new GenerationStoppingCriterion(tinyGp)
            {
                Limit = 50
            },
            new FitnessBound()
        ];
        await tinyGp.Run();
    }
}