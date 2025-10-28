using Italbytz.AI;
using Italbytz.EA;
using Italbytz.EA.Fitness;
using Italbytz.EA.Graph;
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
    [TestCategory("FixedSeed")]
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
        tinyGp.Initialization = new RandomInitialization
        {
            Size = 10000,
            SearchSpace = tinyGp.SearchSpace
        };

        tinyGp.StoppingCriteria =
        [
            new GenerationStoppingCriterion(tinyGp)
            {
                Limit = 10
            },
            new FitnessBound()
        ];
        var population = await tinyGp.Run();
        Console.WriteLine(population.First());
        var lastFitness = population.First().LatestKnownFitness;
        Assert.IsTrue(lastFitness != null);
        Assert.IsTrue(Math.Abs(lastFitness.ConsolidatedValue + 34.72) < 0.02);
    }
}