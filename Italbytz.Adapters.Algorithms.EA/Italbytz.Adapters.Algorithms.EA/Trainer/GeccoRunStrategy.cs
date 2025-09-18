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
    private Dictionary<uint, int> _labelMapping = new();

    private Dictionary<int, float>[] _reverseFeatureValueMappings =
        Array.Empty<Dictionary<int, float>>();

    
    public int Classes => _labelMapping.Count;
    public Dictionary<int, uint> ReverseLabelMapping { get; set; } = new();
    public Dictionary<float, int>[] FeatureValueMappings { get; set; } = Array.Empty<Dictionary<float, int>>();

    public IIndividual Run(IDataView input)
    {
        var excerpt = input.GetDataExcerpt();
        var features = excerpt.Features;
        var labels = excerpt.Labels;
        _labelMapping = CreateLabelMapping(labels);
        FeatureValueMappings = CreateFeatureValueMappings(features);
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

    private Dictionary<float, int>[] CreateFeatureValueMappings(
        List<float[]> features)
    {
        if (features.Count == 0)
            return Array.Empty<Dictionary<float, int>>();

        var columnCount = features[0].Length;
        var mappings = new Dictionary<float, int>[columnCount];
        _reverseFeatureValueMappings = new Dictionary<int, float>[columnCount];

        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            var columnValues =
                features.Select(row => row[columnIndex]).ToArray();
            var uniqueValues = new HashSet<float>(columnValues);
            var categoryList = uniqueValues.OrderBy(c => c).ToList();
            var mapping = new Dictionary<float, int>();
            var reverseMapping = new Dictionary<int, float>();
            for (var i = 0; i < categoryList.Count; i++)
            {
                mapping[categoryList[i]] = i;
                reverseMapping[i] = categoryList[i];
            }

            mappings[columnIndex] = mapping;
            _reverseFeatureValueMappings[columnIndex] = reverseMapping;
        }

        return mappings;
    }

    private Dictionary<uint, int> CreateLabelMapping(List<uint> labels)
    {
        var uniqueLabels = labels.Distinct().OrderBy(l => l).ToList();
        var mapping = new Dictionary<uint, int>();
        for (var i = 0; i < uniqueLabels.Count; i++)
            mapping[uniqueLabels[i]] = i;
        ReverseLabelMapping =
            mapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        return mapping;
    }

    private int[][] PrepareForLogicGp(List<float[]> features)
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