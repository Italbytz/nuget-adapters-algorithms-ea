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
    protected Dictionary<float, int>[] FeatureValueMappings;
    protected Dictionary<uint, int> LabelMapping;

    public abstract IIndividual Run(IDataView input,
        Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping);

    protected virtual int[][] PrepareForLogicGp(List<float[]> features)
    {
        var result = new int[features.Count][];
        for (var i = 0; i < features.Count; i++)
        {
            var featureRow = features[i];
            var intRow = new int[featureRow.Length];
            for (var j = 0; j < featureRow.Length; j++)
                intRow[j] = FeatureValueMappings[j][featureRow[j]];
            result[i] = intRow;
        }

        return result;
    }


    protected virtual int[] PrepareForLogicGp(List<uint> labels)
    {
        var result = new int[labels.Count];
        for (var i = 0; i < labels.Count; i++)
            result[i] = LabelMapping[labels[i]];
        return result;
    }

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
                new LogicGpSearchSpace<int>(features, labels)
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