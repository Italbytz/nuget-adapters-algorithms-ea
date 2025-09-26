using System.Collections.Generic;
using System.Linq;
using Italbytz.EA.Individuals;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class LogicGpSearchSpace<TCategory> : ISearchSpace
{
    private readonly TCategory[][] _features;
    private readonly TCategory[] _labels;

    public LogicGpSearchSpace(TCategory[][] features, TCategory[] labels)
    {
        _features = features;
        _labels = labels;
        GenerateLiterals();
    }

    public Weighting Weighting { get; set; } = Weighting.Fixed;

    public List<ILiteral<TCategory>> Literals { get; set; }

    public IGenotype GetRandomGenotype()
    {
        return Genotype<TCategory>.GenerateRandomGenotype(Literals,
            Weighting);
    }

    public IIndividualList GetAStartingPopulation()
    {
        var result = new ListBasedPopulation();
        foreach (var literal in Literals)
        {
            var monomial = new WeightedMonomial<TCategory>([literal]);
            var polynomial = new WeightedPolynomial<TCategory>(
                [monomial]);
            var genotype =
                new Genotype<TCategory>(polynomial, Literals, Weighting);
            var individual = new Individual(genotype, null);
            result.Add(individual);
        }

        return result;
    }

    private void GenerateLiterals()
    {
        var uniqueLabelSet = new HashSet<TCategory>(_labels);
        var uniqueLabels = uniqueLabelSet.OrderBy(c => c);
        Literals = new List<ILiteral<TCategory>>();
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
                Literals.Add(literal);
            }
        }
    }
}