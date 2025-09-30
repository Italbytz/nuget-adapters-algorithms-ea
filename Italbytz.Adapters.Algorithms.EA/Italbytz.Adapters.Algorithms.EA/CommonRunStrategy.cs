using System.Collections.Generic;
using System.Threading.Tasks;
using Italbytz.EA.Control;
using Italbytz.EA.Fitness;
using Italbytz.EA.Graph;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.EA.Searchspace;
using Italbytz.EA.StoppingCriterion;
using Microsoft.ML;

namespace Italbytz.EA;

public abstract class CommonRunStrategy : IRunStrategy
{
    public abstract IIndividual Run(IDataView input,
        Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping);


    protected abstract Task<IIndividualList> RunSpecificLogicGp(
        int[][] features,
        int[] labels);

    protected virtual Task<IIndividualList> RunLogicGp(int[][] features,
        int[] labels, OperatorGraph algorithmGraph,
        IInitialization initialization,
        int maxModelSize = int.MaxValue,
        Weighting weighting = Weighting.Computed, int generations = 100)
    {
        var logicGp = new EvolutionaryAlgorithm
        {
            FitnessFunction =
                new ConfusionAndSizeFitnessFunction<int>(features, labels)
                {
                    MaxSize = maxModelSize
                },
            SearchSpace =
                new LogicGpSearchSpace<int>(features)
                {
                    Weighting = weighting
                },
            AlgorithmGraph = algorithmGraph
        };
        logicGp.Initialization = initialization;
        initialization.SearchSpace = logicGp.SearchSpace;

        logicGp.StoppingCriteria =
        [
            new GenerationStoppingCriterion(logicGp)
            {
                Limit = generations
            }
        ];
        var population = logicGp.Run();
        return population;
    }
}