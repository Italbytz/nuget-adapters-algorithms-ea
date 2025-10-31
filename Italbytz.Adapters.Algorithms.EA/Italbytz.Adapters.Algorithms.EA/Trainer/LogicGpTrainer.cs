using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
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
    [JsonInclude] private Dictionary<int, uint> _reverseLabelMapping;

    private int _classes => _labelMapping.Count;

    [JsonIgnore] public IRunStrategy? RunStrategy { get; set; }


    [JsonIgnore] public IIndividualList FinalPopulation { get; set; }
    public IIndividual Model { get; set; }

    public void Save(Stream stream)
    {
        var trainerJson = JsonSerializer.Serialize(this, GetType(),
            new JsonSerializerOptions
            {
                WriteIndented = false,
                Converters = { new ModelJsonConverter() },
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            });
        /*var genotypeJson = JsonSerializer.Serialize(Model.Genotype,
            Model.Genotype.GetType(),
            new JsonSerializerOptions { WriteIndented = false });

        var json =
            $"{{\"Model\":{{\"Genotype\":{genotypeJson}}},{trainerJson[1..]}";*/

        var json = JsonSerializer.Serialize(
            JsonSerializer.Deserialize<object>(trainerJson),
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

    public static LogicGpTrainer<TOutput>? Load(Stream stream)
    {
        var trainerJson =
            JsonSerializer.Deserialize<LogicGpLoadedTrainer<TOutput>>(
                stream,
                new JsonSerializerOptions
                {
                    Converters = { new ModelJsonConverter() }
                });
        return trainerJson;
    }
}

public sealed class ModelJsonConverter : JsonConverter<IIndividual>
{
    public override IIndividual? Read(ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        IGenotype? genotype = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            var propertyName = reader.GetString()!;
            reader.Read();

            if (propertyName == "Genotype")
            {
                // Deserialize the genotype
                var genotypeElement =
                    JsonDocument.ParseValue(ref reader).RootElement;

                //var genotypeTypeProperty = genotypeElement.GetProperty("Type");
                //var genotypeTypeName = genotypeTypeProperty.GetString();
                var genotypeType =
                    typeof(WeightedPolynomialGenotype<SetLiteral<int>, int>);
                /*if (genotypeTypeName != null)
                    genotypeType = Type.GetType(genotypeTypeName);

                if (genotypeType == null)
                    throw new JsonException(
                        $"Unknown genotype type: {genotypeTypeName}");*/

                genotype = (IGenotype)JsonSerializer.Deserialize(
                    genotypeElement.GetRawText(),
                    genotypeType,
                    options)!;
            }
            else
            {
                // Skip unknown properties
                reader.Skip();
            }
        }

        if (genotype == null)
            throw new JsonException("Genotype property is missing.");

        return new Individual(genotype, null);
    }

    public override void Write(Utf8JsonWriter writer, IIndividual value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("Genotype");
        var genotypeJson = JsonSerializer.Serialize(value.Genotype,
            value.Genotype.GetType(),
            new JsonSerializerOptions { WriteIndented = false });
        using (var doc = JsonDocument.Parse(genotypeJson))
        {
            doc.RootElement.WriteTo(writer);
        }

        writer.WriteEndObject();
    }
}