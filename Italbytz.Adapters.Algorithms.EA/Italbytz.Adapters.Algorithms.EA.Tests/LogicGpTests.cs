using Italbytz.AI;
using Italbytz.EA;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.EA.Searchspace;
using Italbytz.EA.Trainer.Gecco;
using GenerationStoppingCriterion =
    Italbytz.EA.StoppingCriterion.GenerationStoppingCriterion;

namespace Italbytz.Adapters.Algorithms.EA.Tests;

[TestClass]
public class LogicGpTests
{
    private int[][] _testFeatures;
    private int[] _testLabels;

    private int[][] _trainingFeatures =
    [
        [0, 3, 0, 0],
        [0, 1, 0, 0],
        [0, 2, 0, 0],
        [0, 2, 0, 0],
        [0, 3, 0, 0],
        [1, 3, 1, 1],
        [0, 3, 0, 0],
        [0, 3, 0, 0],
        [0, 1, 0, 0],
        [0, 2, 0, 0],
        [1, 3, 0, 0],
        [0, 3, 1, 0],
        [0, 1, 0, 0],
        [0, 1, 0, 0],
        [1, 3, 0, 0],
        [1, 3, 0, 1],
        [1, 3, 0, 1],
        [0, 3, 0, 0],
        [1, 3, 1, 0],
        [0, 3, 0, 0],
        [1, 3, 1, 0],
        [0, 3, 0, 1],
        [0, 3, 0, 0],
        [0, 2, 1, 1],
        [0, 3, 1, 0],
        [0, 1, 1, 0],
        [0, 3, 1, 1],
        [1, 3, 0, 0],
        [1, 3, 0, 0],
        [0, 2, 1, 0],
        [0, 2, 1, 0],
        [1, 3, 0, 1],
        [1, 3, 0, 0],
        [1, 3, 0, 0],
        [0, 2, 0, 0],
        [0, 2, 0, 0],
        [1, 3, 0, 0],
        [0, 2, 0, 0],
        [0, 1, 0, 0],
        [0, 3, 0, 0],
        [0, 3, 0, 0],
        [0, 0, 0, 0],
        [0, 2, 0, 0],
        [0, 3, 1, 1],
        [0, 3, 1, 1],
        [0, 1, 0, 0],
        [0, 3, 1, 0],
        [0, 2, 0, 0],
        [1, 3, 0, 0],
        [0, 2, 0, 0],
        [3, 2, 2, 2],
        [2, 2, 2, 2],
        [3, 2, 2, 2],
        [1, 0, 1, 1],
        [3, 0, 2, 2],
        [1, 0, 2, 1],
        [2, 2, 2, 2],
        [0, 0, 1, 1],
        [3, 1, 2, 1],
        [1, 0, 1, 2],
        [0, 0, 1, 1],
        [2, 1, 1, 2],
        [2, 0, 1, 1],
        [2, 1, 2, 2],
        [1, 1, 1, 1],
        [3, 2, 2, 2],
        [1, 1, 2, 2],
        [1, 0, 1, 1],
        [2, 0, 2, 2],
        [1, 0, 1, 1],
        [2, 2, 2, 2],
        [2, 0, 1, 1],
        [2, 0, 2, 2],
        [2, 0, 2, 1],
        [2, 1, 1, 1],
        [3, 1, 2, 2],
        [3, 0, 2, 2],
        [3, 1, 2, 2],
        [2, 1, 2, 2],
        [1, 0, 1, 1],
        [1, 0, 1, 1],
        [1, 0, 1, 1],
        [1, 0, 1, 1],
        [2, 0, 2, 2],
        [1, 1, 2, 2],
        [2, 3, 2, 2],
        [3, 2, 2, 2],
        [2, 0, 2, 1],
        [1, 1, 1, 1],
        [1, 0, 1, 1],
        [1, 0, 2, 1],
        [2, 1, 2, 2],
        [1, 0, 1, 1],
        [0, 0, 1, 1],
        [1, 0, 1, 1],
        [1, 1, 1, 1],
        [1, 1, 1, 1],
        [2, 1, 1, 1],
        [0, 0, 1, 1],
        [1, 0, 1, 1],
        [2, 2, 3, 3],
        [1, 0, 2, 3],
        [3, 1, 3, 3],
        [2, 1, 3, 2],
        [3, 1, 3, 3],
        [3, 1, 3, 3],
        [0, 0, 2, 2],
        [3, 1, 3, 2],
        [3, 0, 3, 2],
        [3, 3, 3, 3],
        [3, 2, 2, 3],
        [2, 0, 3, 3],
        [3, 1, 3, 3],
        [1, 0, 2, 3],
        [1, 0, 2, 3],
        [2, 2, 3, 3],
        [3, 1, 3, 2],
        [3, 3, 3, 3],
        [3, 0, 3, 3],
        [2, 0, 2, 2],
        [3, 2, 3, 3],
        [1, 0, 2, 3],
        [3, 0, 3, 3],
        [2, 0, 2, 2],
        [3, 2, 3, 3],
        [3, 2, 3, 2],
        [2, 0, 2, 2],
        [2, 1, 2, 2],
        [2, 0, 3, 3],
        [3, 1, 3, 2],
        [3, 0, 3, 3],
        [3, 3, 3, 3],
        [2, 0, 3, 3],
        [2, 0, 2, 2],
        [2, 0, 3, 2],
        [3, 1, 3, 3],
        [2, 3, 3, 3],
        [2, 2, 3, 2],
        [2, 1, 2, 2],
        [3, 2, 3, 3],
        [3, 2, 3, 3],
        [3, 2, 2, 3],
        [1, 0, 2, 3],
        [3, 2, 3, 3],
        [3, 2, 3, 3],
        [3, 1, 3, 3],
        [2, 0, 2, 3],
        [3, 1, 3, 3],
        [2, 3, 3, 3],
        [2, 1, 2, 2]
    ];

    private int[] _trainingLabels =
    [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2
    ];

    [TestInitialize]
    public void Initialize()
    {
        // Kopieren der ursprünglichen Daten
        var featuresList = _trainingFeatures.ToList();
        var labelsList = _trainingLabels.ToList();

        _testFeatures = new int[20][];
        _testLabels = new int[20];

        // 20 zufällige Elemente auswählen und in die Testdaten verschieben
        for (var i = 0; i < 20; i++)
        {
            var index = ThreadSafeRandomNetCore.Shared.Next(featuresList.Count);
            _testFeatures[i] = featuresList[index];
            _testLabels[i] = labelsList[index];

            featuresList.RemoveAt(index);
            labelsList.RemoveAt(index);
        }

        // Restliche Daten in _trainingFeatures und _trainingLabels zurückschreiben
        _trainingFeatures = featuresList.ToArray();
        _trainingLabels = labelsList.ToArray();
    }

    [TestMethod]
    [TestCategory("FixedSeed")]
    public async Task TestLogicGp()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        var logicGp = new EvolutionaryAlgorithm
        {
            FitnessFunction =
                new ConfusionAndSizeFitnessFunction<int>(_trainingFeatures,
                    _trainingLabels)
                {
                    MaxSize = int.MaxValue
                },
            SearchSpace =
                new LogicGpSearchSpace<int>(_trainingFeatures)
                {
                    Weighting = Weighting.Computed
                },
            AlgorithmGraph = new LogicGpGraph()
        };
        logicGp.Initialization = new RandomInitialization
        {
            Size = 10,
            SearchSpace = logicGp.SearchSpace
        };

        logicGp.StoppingCriteria =
        [
            new GenerationStoppingCriterion(logicGp)
            {
                Limit = 100
            }
        ];
        var population = await logicGp.Run();
        Console.Out.WriteLine(population);
        Console.Out.WriteLine("################");
        population.Freeze();
        var fitness =
            new ConfusionAndSizeFitnessFunction<int>(_testFeatures,
                _testLabels);
        foreach (var individual in population)
        {
            var newFitness = fitness.Evaluate(individual);
            if (individual.Genotype is IValidatableGenotype genotype)
            {
                genotype.TrainingFitness = individual.LatestKnownFitness;
                genotype.ValidationFitness = newFitness;
            }

            individual.LatestKnownFitness = newFitness;
        }

        Console.Out.WriteLine(population);
    }
}