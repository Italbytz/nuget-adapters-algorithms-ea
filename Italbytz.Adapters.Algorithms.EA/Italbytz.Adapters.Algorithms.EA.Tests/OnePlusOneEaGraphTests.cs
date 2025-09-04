using Italbytz.EA.Control;
using Italbytz.EA.Fitness;
using Italbytz.EA.Graph.Common;
using Italbytz.EA.Initialization;
using Italbytz.EA.Searchspace;
using Italbytz.EA.StoppingCriterion;

namespace Italbytz.AI.Tests.Unit.Search.EA;

[TestClass]
public class OnePlusOneEaGraphTests
{
    [TestMethod]
    public async Task TestOnePlusOneEA()
    {
        var onePlusOneEA = new EvolutionaryAlgorithm
        {
            FitnessFunction = new OneMax(),
            SearchSpace = new BitString(),
            AlgorithmGraph = new OnePlusOneEAGraph()
        };
        onePlusOneEA.Initialization = new RandomInitialization(onePlusOneEA);
        onePlusOneEA.StoppingCriteria =
        [
            new GenerationStoppingCriterion(onePlusOneEA)
        ];
        await onePlusOneEA.Run();
    }
}