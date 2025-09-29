using System;
using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Initialization;

/// <inheritdoc cref="IInitialization" />
public class CompleteInitialization
    : IInitialization
{
    public Task<IIndividualList>? Process(Task<IIndividualList> individuals,
        IFitnessFunction fitnessFunction)
    {
        var population = SearchSpace.GetAStartingPopulation();
        return Task.FromResult(population);
    }

    public object Clone()
    {
        throw new NotImplementedException();
    }

    public ISearchSpace SearchSpace { get; set; }
}