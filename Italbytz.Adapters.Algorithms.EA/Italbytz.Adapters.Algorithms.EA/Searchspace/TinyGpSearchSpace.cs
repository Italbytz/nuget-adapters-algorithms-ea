using System;
using Italbytz.EA.Individuals;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class TinyGpSearchSpace : ISearchSpace
{
    public int Depth { get; set; } = 5;

    public int MaxLen { get; set; } = 10000;


    public IGenotype GetRandomGenotype()
    {
        return TinyGpGenotype.GenerateRandomGenotype(MaxLen, Depth);
    }

    public IIndividualList GetAStartingPopulation()
    {
        throw new NotImplementedException();
    }
}