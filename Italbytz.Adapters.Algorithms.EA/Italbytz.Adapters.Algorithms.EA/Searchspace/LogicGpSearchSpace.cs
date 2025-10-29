using System.Collections.Generic;
using System.Linq;
using Italbytz.EA.Individuals;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class LogicGpSearchSpace<TCategory> : ISearchSpace
{
    private readonly TCategory[][] _features;
    private readonly int[] _labels;

    public LogicGpSearchSpace(TCategory[][] features, int[] labels,
        double minMaxWeight = 0.0)
    {
        _features = features;
        _labels = labels;
        GenerateLiterals(minMaxWeight);
    }

    public Weighting Weighting { get; set; } = Weighting.Fixed;

    public IList<SetLiteral<TCategory>> Literals { get; set; }

    public IGenotype GetRandomGenotype()
    {
        return WeightedPolynomialGenotype<SetLiteral<TCategory>, TCategory>
            .GenerateRandomGenotype(Literals,
                Weighting);
    }

    public IIndividualList GetAStartingPopulation()
    {
        var result = new ListBasedPopulation();
        foreach (var literal in Literals)
        {
            var monomial =
                new WeightedMonomial<SetLiteral<TCategory>, TCategory>(
                    [literal]);
            var polynomial =
                new WeightedPolynomial<SetLiteral<TCategory>, TCategory>(
                    [monomial]);
            var genotype =
                new WeightedPolynomialGenotype<SetLiteral<TCategory>,
                    TCategory>(polynomial, Literals,
                    Weighting);
            var individual = new Individual(genotype, null);
            result.Add(individual);
        }

        return result;
    }

    private void GenerateLiterals(double minMaxWeight = 0.0)
    {
        Literals = [];
        if (_features.Length == 0) return;

        var columnCount = _features[0].Length;
        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            // Extrahiere alle Werte für diese Spalte aus allen Zeilen
            var columnValues =
                _features.Select(row => row[columnIndex]).ToArray();
            var uniqueValues = new HashSet<TCategory>(columnValues);
            var categoryList = uniqueValues.OrderBy(c => c).ToList();
            var uniqueCount = uniqueValues.Count;
            var powerSetCount = 1 << uniqueCount;
            for (var i = 1; i < powerSetCount - 1; i++)
            {
                var literalType = uniqueValues.Count <= 3
                    ? SetLiteralType.Dussault
                    : SetLiteralType.Rudell;
                var literal =
                    new SetLiteral<TCategory>(columnIndex, categoryList, i,
                        literalType);
                if (minMaxWeight > 0)
                {
                    var genotype =
                        new WeightedPolynomialGenotype<SetLiteral<TCategory>,
                            TCategory>(literal, null,
                            Weighting.Computed);
                    genotype.ComputeWeights(_features, _labels);
                    var weights = genotype.Polynomial.Monomials[0].Weights;
                    if (weights.Any(t => t > minMaxWeight))
                        Literals.Add(literal);
                }
                else
                {
                    Literals.Add(literal);
                }
            }
        }
    }
}