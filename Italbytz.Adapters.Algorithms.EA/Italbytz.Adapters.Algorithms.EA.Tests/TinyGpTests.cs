using Italbytz.AI;
using Italbytz.EA.Control;
using Italbytz.EA.Fitness;
using Italbytz.EA.GP.StoppingCriterion;
using Italbytz.EA.Graph.Common;
using Italbytz.EA.Initialization;
using Italbytz.EA.Searchspace;
using GenerationStoppingCriterion =
    Italbytz.EA.StoppingCriterion.GenerationStoppingCriterion;

namespace Italbytz.Adapters.Algorithms.EA.Tests;

[TestClass]
public class TinyGpTests
{
    private readonly double[][] _features =
        Enumerable.Range(0, 63)
            .Select(i => new[] { 0.1 * i })
            .ToArray();

    private readonly double[] _labels = Enumerable.Range(0, 63)
        .Select(i => Math.Sin(0.1 * i)).ToArray();

    [TestMethod]
    public async Task TestTinyGp()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        var tinyGp = new EvolutionaryAlgorithm
        {
            FitnessFunction = new AbsoluteDeviation(_features, _labels),
            SearchSpace = new TinyGpSearchSpace(),
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
                Limit = 5
            },
            new FitnessBound()
        ];
        await tinyGp.Run();
    }
}