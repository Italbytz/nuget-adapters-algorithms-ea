using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.EA.Individuals;
using Italbytz.ML;
using Italbytz.ML.Trainers;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.EA.Trainer;

public class LogicGpTrainer<TOutput> : CustomClassificationTrainer<TOutput>
    where TOutput : class, new()
{
    private Dictionary<float, int>[] _featureValueMappings;
    private Dictionary<uint, int> _labelMapping = new();
    private IIndividual? _model;
    private Dictionary<int, float>[] _reverseFeatureValueMappings;
    private Dictionary<int, uint> _reverseLabelMapping;
    private int _classes => _labelMapping.Count;

    public IRunStrategy RunStrategy { get; set; } = new GeccoRunStrategy();

    protected override void Map(ClassificationInput input, TOutput output)
    {
        if (_model == null)
            throw new InvalidOperationException("Model is not trained.");
        var featureArray = input.Features.ToArray();
        var intFeatures = new int[featureArray.Length];
        for (var i = 0; i < featureArray.Length; i++)
        {
            if (_featureValueMappings.Length <= i ||
                !_featureValueMappings[i].TryGetValue(featureArray[i],
                    out var intValue))
                // Handle unknown feature value
                intValue = -1; // or some other default value

            intFeatures[i] = intValue;
        }

        if (_model.Genotype is not IPredictingGenotype<int> genotype)
            throw new InvalidOperationException(
                "Model genotype does not support prediction.");
        var prediction = genotype.PredictClass(intFeatures);
        if (!_reverseLabelMapping.TryGetValue(prediction, out var label))
            throw new InvalidOperationException(
                "Predicted label not found in reverse mapping.");
        switch (output)
        {
            case IBinaryClassificationOutput binaryOutput:
                binaryOutput.PredictedLabel =
                    label;
                binaryOutput.Score =
                    prediction == 1
                        ? 0f
                        : 1f;
                binaryOutput.Probability =
                    prediction == 1
                        ? 0f
                        : 1f;
                break;
            case IMulticlassClassificationOutput multiclassOutput:
            {
                multiclassOutput.PredictedLabel = label;
                var scores = new float[_classes];
                scores[label - 1] = 1f;
                var probabilities = new float[_classes];
                probabilities[label - 1] = 1f;
                multiclassOutput.Score =
                    new VBuffer<float>(scores.Length, scores);
                multiclassOutput.Probability =
                    new VBuffer<float>(probabilities.Length, probabilities);
                break;
            }
            default:
                throw new ArgumentException(
                    "The destination is not of type IBinaryClassificationOutput or IMulticlassClassificationOutput");
        }
    }

    protected override void PrepareForFit(IDataView input)
    {
        var excerpt = input.GetDataExcerpt();
        var features = excerpt.Features;
        var labels = excerpt.Labels;
        _labelMapping = CreateLabelMapping(labels);
        _featureValueMappings = CreateFeatureValueMappings(features);
        _model = RunStrategy.Run(input, _featureValueMappings,
            _labelMapping);
        Console.WriteLine(_model);
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
        _reverseLabelMapping =
            mapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        return mapping;
    }
}