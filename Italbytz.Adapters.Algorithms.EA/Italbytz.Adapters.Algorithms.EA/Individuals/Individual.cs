using System;
using System.Globalization;
using System.Linq;
using Italbytz.EA.Fitness;

namespace Italbytz.EA.Individuals;

/// <inheritdoc cref="IIndividual" />
public class Individual : IIndividual
{
    public Individual(IGenotype genotype, IIndividual[]? parents)
    {
        Genotype = genotype;
        Parents = parents;
    }

    public IIndividual[]? Parents { get; set; }

    /// <inheritdoc />
    public IGenotype Genotype { get; }

    /// <inheritdoc />
    public IFitnessValue? LatestKnownFitness
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
        return fitness.IsDominating(otherFitness);
    }

    /// <inheritdoc />
    public object Clone()
    {
        return new Individual((IGenotype)Genotype.Clone(), [this])
        {
            Generation = Generation
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Genotype +
               $" Generation {Generation}, " +
               $"Fitness {LatestKnownFitness}" ??
               string.Empty;
    }
}