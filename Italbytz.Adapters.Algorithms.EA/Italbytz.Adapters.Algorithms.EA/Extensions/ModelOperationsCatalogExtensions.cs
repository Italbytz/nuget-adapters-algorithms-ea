using System;
using System.IO;
using System.Text.Json;
using Italbytz.EA.Searchspace;
using Microsoft.ML;

namespace Italbytz.ML;

public static class ModelOperationsCatalogExtensions
{
    // ToDo: To obtain a transformer, we also need to deserialize the trainer to get the feature and label mappings. Additionally, the preprocessing steps need to be reapplied.
    public static ITransformer Load(this ModelOperationsCatalog catalog,
        string fileName)
    {
        try
        {
            var model = catalog.Load(fileName, out _);
            return model;
        }
        catch (FormatException ex)
        {
            //PolynomialGenotype<int> genotype
            try
            {
                var json = File.ReadAllText(fileName);
                var genotype =
                    JsonSerializer
                        .Deserialize<WeightedPolynomialGenotype<SetLiteral<int>,
                            int>>(
                            json,
                            new JsonSerializerOptions
                                { PropertyNameCaseInsensitive = true });
                if (genotype == null)
                    throw new InvalidOperationException(
                        $"Failed to deserialize {fileName} as PolynomialGenotype<int>");
            }
            catch (Exception e)
            {
                e.ToString();
            }
        }

        throw new InvalidOperationException(
            $"Failed to load model from file: {fileName}");
    }
}