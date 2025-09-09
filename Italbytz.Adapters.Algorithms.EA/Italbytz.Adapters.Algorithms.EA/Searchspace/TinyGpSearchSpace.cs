using System;
using Italbytz.AI;
using Italbytz.EA.Individuals;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class TinyGpSearchSpace : ISearchSpace
{
    public TinyGpSearchSpace()
    {
        for (var i = 0; i < NumberConst; i++)
            Constants[i] =
                ThreadSafeRandomNetCore.LocalRandom.NextDouble() *
                (MaxRandom - MinRandom) + MinRandom;
    }

    public int Depth { get; set; } = 5;

    public int MaxLen { get; set; } = 10000;

    public int MinRandom { get; set; } = -5;
    public int MaxRandom { get; set; } = 5;

    public int VariableCount { get; set; } = 1;
    public int NumberConst { get; set; } = 100;

    public double[] Constants { get; set; } = new double[100];

    public IGenotype GetRandomGenotype()
    {
        return TinyGpGenotype.GenerateRandomGenotype(MaxLen, Depth, Constants);
    }

    public IIndividualList GetAStartingPopulation()
    {
        throw new NotImplementedException();
    }
}