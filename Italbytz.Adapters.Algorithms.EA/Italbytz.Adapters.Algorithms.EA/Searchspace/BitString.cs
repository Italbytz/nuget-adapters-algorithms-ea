using System;
using System.Collections;
using Italbytz.AI;
using Italbytz.EA.Individuals;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class BitString : ISearchSpace
{
    public BitString(int dimension = 64)
    {
        Dimension = dimension;
    }

    public int Dimension { get; } = 64;

    public IGenotype GetRandomGenotype()
    {
        var bs = new BitArray(Dimension);
        var random = ThreadSafeRandomNetCore.Shared;
        for (var i = 0; i < Dimension; i++) bs[i] = random.NextDouble() < 0.5;
        return new BitStringGenotype(bs, Dimension);
    }

    public IIndividualList GetAStartingPopulation()
    {
        throw new NotImplementedException();
    }
}