using System;
using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Initialization;

public class RandomInitialization
    : IInitialization
{
    public int Size { get; set; } = 1;
    public ISearchSpace SearchSpace { get; set; }

    public Task<IIndividualList>? Process(Task<IIndividualList> individuals,
        IFitnessFunction fitnessFunction)
    {
        var result = new ListBasedPopulation();
        for (var i = 0; i < Size; i++)
            result
                .Add(new Individual(SearchSpace.GetRandomGenotype(),
                    null));
        return Task.FromResult<IIndividualList>(result);
    }

    public object Clone()
    {
        throw new NotImplementedException();
    }
}