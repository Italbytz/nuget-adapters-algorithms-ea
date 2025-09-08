using Italbytz.AI;
using Italbytz.EA.Control;
using Italbytz.EA.Fitness;
using Italbytz.EA.Graph.Common;
using Italbytz.EA.Initialization;
using Italbytz.EA.Searchspace;
using Italbytz.EA.StoppingCriterion;
using Microsoft.ML;

namespace Italbytz.Adapters.Algorithms.EA.Tests;

[TestClass]
public class TinyGpTests
{
    private readonly IDataView sin = new MLContext().Data.LoadFromEnumerable(
    [
        new { X = 0.0f, Label = (float)Math.Sin(0.0) },
        new { X = 0.1f, Label = (float)Math.Sin(0.1) },
        new { X = 0.2f, Label = (float)Math.Sin(0.2) },
        new { X = 0.3f, Label = (float)Math.Sin(0.3) },
        new { X = 0.4f, Label = (float)Math.Sin(0.4) },
        new { X = 0.5f, Label = (float)Math.Sin(0.5) },
        new { X = 0.6f, Label = (float)Math.Sin(0.6) },
        new { X = 0.7f, Label = (float)Math.Sin(0.7) },
        new { X = 0.8f, Label = (float)Math.Sin(0.8) },
        new { X = 0.9f, Label = (float)Math.Sin(0.9) },
        new { X = 1.0f, Label = (float)Math.Sin(1.0) },
        new { X = 1.1f, Label = (float)Math.Sin(1.1) },
        new { X = 1.2f, Label = (float)Math.Sin(1.2) },
        new { X = 1.3f, Label = (float)Math.Sin(1.3) },
        new { X = 1.4f, Label = (float)Math.Sin(1.4) },
        new { X = 1.5f, Label = (float)Math.Sin(1.5) },
        new { X = 1.6f, Label = (float)Math.Sin(1.6) },
        new { X = 1.7f, Label = (float)Math.Sin(1.7) },
        new { X = 1.8f, Label = (float)Math.Sin(1.8) },
        new { X = 1.9f, Label = (float)Math.Sin(1.9) },
        new { X = 2.0f, Label = (float)Math.Sin(2.0) },
        new { X = 2.1f, Label = (float)Math.Sin(2.1) },
        new { X = 2.2f, Label = (float)Math.Sin(2.2) },
        new { X = 2.3f, Label = (float)Math.Sin(2.3) },
        new { X = 2.4f, Label = (float)Math.Sin(2.4) },
        new { X = 2.5f, Label = (float)Math.Sin(2.5) },
        new { X = 2.6f, Label = (float)Math.Sin(2.6) },
        new { X = 2.7f, Label = (float)Math.Sin(2.7) },
        new { X = 2.8f, Label = (float)Math.Sin(2.8) },
        new { X = 2.9f, Label = (float)Math.Sin(2.9) },
        new { X = 3.0f, Label = (float)Math.Sin(3.0) },
        new { X = 3.1f, Label = (float)Math.Sin(3.1) },
        new { X = 3.2f, Label = (float)Math.Sin(3.2) },
        new { X = 3.3f, Label = (float)Math.Sin(3.3) },
        new { X = 3.4f, Label = (float)Math.Sin(3.4) },
        new { X = 3.5f, Label = (float)Math.Sin(3.5) },
        new { X = 3.6f, Label = (float)Math.Sin(3.6) },
        new { X = 3.7f, Label = (float)Math.Sin(3.7) },
        new { X = 3.8f, Label = (float)Math.Sin(3.8) },
        new { X = 3.9f, Label = (float)Math.Sin(3.9) },
        new { X = 4.0f, Label = (float)Math.Sin(4.0) },
        new { X = 4.1f, Label = (float)Math.Sin(4.1) },
        new { X = 4.2f, Label = (float)Math.Sin(4.2) },
        new { X = 4.3f, Label = (float)Math.Sin(4.3) },
        new { X = 4.4f, Label = (float)Math.Sin(4.4) },
        new { X = 4.5f, Label = (float)Math.Sin(4.5) },
        new { X = 4.6f, Label = (float)Math.Sin(4.6) },
        new { X = 4.7f, Label = (float)Math.Sin(4.7) },
        new { X = 4.8f, Label = (float)Math.Sin(4.8) },
        new { X = 4.9f, Label = (float)Math.Sin(4.9) },
        new { X = 5.0f, Label = (float)Math.Sin(5.0) },
        new { X = 5.1f, Label = (float)Math.Sin(5.1) },
        new { X = 5.2f, Label = (float)Math.Sin(5.2) },
        new { X = 5.3f, Label = (float)Math.Sin(5.3) },
        new { X = 5.4f, Label = (float)Math.Sin(5.4) },
        new { X = 5.5f, Label = (float)Math.Sin(5.5) },
        new { X = 5.6f, Label = (float)Math.Sin(5.6) },
        new { X = 5.7f, Label = (float)Math.Sin(5.7) },
        new { X = 5.8f, Label = (float)Math.Sin(5.8) },
        new { X = 5.9f, Label = (float)Math.Sin(5.9) },
        new { X = 6.0f, Label = (float)Math.Sin(6.0) },
        new { X = 6.1f, Label = (float)Math.Sin(6.1) },
        new { X = 6.2f, Label = (float)Math.Sin(6.2) }
    ]);

    [TestMethod]
    public async Task TestTinyGp()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        var tinyGp = new EvolutionaryAlgorithm
        {
            FitnessFunction = new Accuracy(sin),
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
                Limit = 50
            }
        ];
        await tinyGp.Run();
    }
}