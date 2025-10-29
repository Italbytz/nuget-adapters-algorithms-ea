using System;
using System.IO;
using Italbytz.EA.Fitness;
using Italbytz.ML;

namespace Italbytz.EA.Individuals;

/// <inheritdoc cref="IIndividual" />
public class Individual : IIndividual, ISaveable
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

    public void Save(Stream stream)
    {
        if (Genotype is ISaveable saveable)
            saveable.Save(stream);
        else
            throw new InvalidOperationException("Genotype is not saveable.");
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