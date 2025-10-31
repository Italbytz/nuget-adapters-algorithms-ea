using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Italbytz.EA.Individuals;
using Italbytz.EA.Searchspace;

namespace Italbytz.EA.Extensions;

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