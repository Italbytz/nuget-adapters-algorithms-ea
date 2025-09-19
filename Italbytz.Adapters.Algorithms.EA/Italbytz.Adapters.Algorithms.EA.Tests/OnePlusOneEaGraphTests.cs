using Italbytz.EA;
using Italbytz.EA.Fitness;
using Italbytz.EA.Graph;
using Italbytz.EA.Initialization;
using Italbytz.EA.Searchspace;
using Italbytz.EA.StoppingCriterion;

namespace Italbytz.AI.Tests.Unit.Search.EA;

[TestClass]
public class OnePlusOneEaGraphTests
{
    [TestMethod]
    [TestCategory("FixedSeed")]
    public async Task TestOnePlusOneEA()
    {
        // ToDo: Fix fixed seed for .NET Core
        ThreadSafeRandomNetCore.Seed = 42;
        var onePlusOneEA = new EvolutionaryAlgorithm
        {
            FitnessFunction = new OneMax(),
            SearchSpace = new BitString(),
            AlgorithmGraph = new OnePlusOneEAGraph()
        };
        onePlusOneEA.Initialization =
            new RandomInitialization(onePlusOneEA.SearchSpace);
        onePlusOneEA.StoppingCriteria =
        [
            new GenerationStoppingCriterion(onePlusOneEA)
            {
                Limit = 100
            }
        ];
        var population = await onePlusOneEA.Run();
        Console.WriteLine(population);
        var lastFitness = population.First().LatestKnownFitness;
        Assert.IsTrue(lastFitness!=null);
    }
}         