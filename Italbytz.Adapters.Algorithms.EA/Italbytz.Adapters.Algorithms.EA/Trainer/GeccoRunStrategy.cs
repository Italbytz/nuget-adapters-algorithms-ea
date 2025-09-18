using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Italbytz.EA.Control;
using Italbytz.EA.Fitness;
using Italbytz.EA.Graph.Common;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.EA.Searchspace;
using Italbytz.EA.StoppingCriterion;
using Italbytz.ML;
using Microsoft.ML;

namespace Italbytz.EA.Trainer;

public class GeccoRunStrategy : IRunStrategy
{
    private Dictionary<float, int>[] _featureValueMappings;
    private Dictionary<uint, int> _labelMapping;

    public IIndividual Run(IDataView input, Dictionary<float, int>[] featureValueMappings, Dictionary<uint, int> labelMapping)
    {
        _featureValueMappings = featureValueMappings;
        _labelMapping = labelMapping;
        var excerpt = input.GetDataExcerpt();
        var features = excerpt.Features;
        var labels = excerpt.Labels;
        
        var convertedLabels = PrepareForLogicGp(labels);
        var convertedFeatures = PrepareForLogicGp(features);
        var individuals = RunLogicGp(convertedFeatures, convertedLabels);
        return ChooseBestIndividual(individuals);
    }
    
    private IIndividual? ChooseBestIndividual(Task<IIndividualList> individuals)
    {
        var population = individuals.Result;
        IIndividual? bestIndividual = null;
        var bestFitness = double.MinValue;
        foreach (var individual in population)
        {
            var fitness = individual.LatestKnownFitness.Sum();
            if (fitness > bestFitness)
            {
                bestFitness = fitness;
                bestIndividual = individual;
            }
        }

        return bestIndividual;
    }

    

    private int[][] PrepareForLogicGp(List<float[]> features)
    {
        var result = new int[features.Count][];
        for (var i = 0; i < features.Count; i++)
        {
            var featureRow = features[i];
            var intRow = new int[featureRow.Length];
            for (var j = 0; j < featureRow.Length; j++)
                intRow[j] = _featureValueMappings[j][featureRow[j]];
            result[i] = intRow;
        }

        return result;
    }


    private int[] PrepareForLogicGp(List<uint> labels)
    {
        var result = new int[labels.Count];
        for (var i = 0; i < labels.Count; i++)
            result[i] = _labelMapping[labels[i]];
        return result;
    }


    private Task<IIndividualList> RunLogicGp(int[][] features, int[] labels)
    {
        var logicGp = new EvolutionaryAlgorithm
        {
            FitnessFunction =
                new LogicGpPareto<int>(features, labels)
                {
                    MaxSize = int.MaxValue
                },
            SearchSpace =
                new LogicGpSearchSpace<int>(features, labels)
                {
                    Weighting = Weighting.Computed
                },
            AlgorithmGraph = new LogicGPGeccoGraph()
        };
        logicGp.Initialization = new RandomInitialization(logicGp.SearchSpace)
        {
            Size = 10
        };

        logicGp.StoppingCriteria =
        [
            new GenerationStoppingCriterion(logicGp)
            {
                Limit = 100
            }
        ];
        var population = logicGp.Run();
        return population;
    }
    
}