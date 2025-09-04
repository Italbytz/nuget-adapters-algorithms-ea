using System.Threading.Tasks;
using Italbytz.EA.Control;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Initialization;

public class RandomInitialization(EvolutionaryAlgorithm schedule)
    : IInitialization
{
    public int Size { get; set; } = 1;

    public Task<IIndividualList>? Process(Task<IIndividualList> individuals,
        IFitnessFunction fitnessFunction)
    {
        var result = new Population();
        var searchSpace = schedule.SearchSpace;
        for (var i = 0; i < Size; i++)
            result
                .Add(new Individual(searchSpace.GetRandomGenotype(),
                    null));
        return Task.FromResult<IIndividualList>(result);
    }
}