using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Italbytz.EA.Control;
using Italbytz.EA.Individuals;
using Italbytz.EA.Searchspace;
using Italbytz.ML;
using Italbytz.ML.Trainers;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.EA.Trainer;

public abstract class LogicGpTrainer<TOutput> :
    CustomClassificationTrainer<TOutput>, IInterpretableTrainer,
    ISaveable
    where TOutput : class, new()

{
    [JsonInclude] private Dictionary<float, int>[] _featureValueMappings;
    [JsonInclude] private Dictionary<uint, int> _labelMapping = new();
    private Dictionary<int, float>[] _reverseFeatureValueMappings;
    private Dictionary<int, uint> _reverseLabelMapping;

    private int _classes => _labelMapping.Count;

    [JsonIgnore] public IRunStrategy? RunStrategy { get; set; }


    [JsonIgnore] public IIndividualList FinalPopulation { get; set; }
    [JsonIgnore] public IIndividual Model { get; set; }

    public void Save(Stream stream)
    {
        var trainerJson = JsonSerializer.Serialize(this, GetType(),
            new JsonSerializerOptions { WriteIndented = false });
        var genotypeJson = JsonSerializer.Serialize(Model.Genotype,
            Model.Genotype.GetType(),
            new JsonSerializerOptions { WriteIndented = false });

        var json =
            $"{{\"Model\":{{\"Genotype\":{genotypeJson}}},{trainerJson[1..]}";

        json = JsonSerializer.Serialize(
            JsonSerializer.Deserialize<object>(json),
            new JsonSerializerOptions { WriteIndented = true });

        var writer = new StreamWriter(stream);
        writer.Write(json);
        writer.Flush();
    }

    protected override void Map(ClassificationInput input, TOutput output)
    {
        if (Model == null)
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

        if (Model.Genotype is not IPredictingGenotype<int> genotype)
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
                        ? 1f
                        : 0f;
                binaryOutput.Probability =
                    prediction == 1
                        ? 1f
                        : 0f;
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
        (_labelMapping, _reverseLabelMapping) =
            MappingHelper.CreateLabelMapping(labels);
        (_featureValueMappings, _reverseFeatureValueMappings) =
            MappingHelper.CreateFeatureValueMappings(features);
        (Model, FinalPopulation) = RunStrategy.Run(input, _featureValueMappings,
            _labelMapping);
    }
}