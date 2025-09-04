using System;
using System.Linq;

namespace Italbytz.EA.Individuals;

/// <inheritdoc cref="IIndividual" />
public class Individual : IIndividual
{
    public Individual(IGenotype genotype, IIndividual[]? parents)
    {
        Genotype = genotype;
    }

    /// <inheritdoc />
    public IGenotype Genotype { get; }

    /// <inheritdoc />
    public double[]? LatestKnownFitness
    {
        get => Genotype.LatestKnownFitness;
        set => Genotype.LatestKnownFitness = value;
    }

    /// <inheritdoc />
    public int Size => Genotype.Size;

    /// <inheritdoc />
    public int Generation { get; set; }

    /// <inheritdoc />
    public bool IsDominating(IIndividual otherIndividual)
    {
        var fitness = LatestKnownFitness;
        var otherFitness = otherIndividual.LatestKnownFitness;
        if (fitness == null || otherFitness == null)
            throw new InvalidOperationException("Fitness not set");
        return !fitness.Where((t, i) => t < otherFitness[i]).Any();
    }

    /// <inheritdoc />
    public object Clone()
    {
        return new Individual((IGenotype)Genotype.Clone(), null);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Genotype + $" Gen {Generation} " +
               $"Fitness {string.Join(",", LatestKnownFitness ?? Array.Empty<double>())}" ??
               string.Empty;
    }
}