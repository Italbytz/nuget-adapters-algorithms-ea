using System;
using System.IO;
using System.Text.Json;
using Italbytz.EA.Searchspace;
using Microsoft.ML;

namespace Italbytz.ML;

public static class ModelOperationsCatalogExtensions
{
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