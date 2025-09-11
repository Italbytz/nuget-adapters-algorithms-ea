using System;
using Italbytz.AI;
using Italbytz.EA.Individuals;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class TinyGpSearchSpace : ISearchSpace
{
    public TinyGpSearchSpace()
    {
        Constants = new double[NumberConst];
        for (var i = 0; i < NumberConst; i++)
            Constants[i] =
                ThreadSafeRandomNetCore.Shared.NextDouble() *
                (MaxRandom - MinRandom) + MinRandom;
    }

    public int Depth { get; init; } = 5;

    public int MaxLen { get; init; } = 10000;

    public int MinRandom { get; init; } = -5;
    public int MaxRandom { get; init; } = 5;

    public int VariableCount { get; init; } = 1;
    public int NumberConst { get; init; } = 100;

    public double[] Constants { get; }

    public IGenotype GetRandomGenotype()
    {
        return TinyGpGenotype.GenerateRandomGenotype(MaxLen, Depth,
            VariableCount, Constants);
    }

    public IIndividualList GetAStartingPopulation()
    {
        throw new NotImplementedException();
    }
}