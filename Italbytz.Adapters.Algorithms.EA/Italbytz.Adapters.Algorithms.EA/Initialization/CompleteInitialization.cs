using System.Threading.Tasks;
using Italbytz.EA.Control;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Initialization;

/// <inheritdoc cref="IInitialization" />
public class CompleteInitialization(EvolutionaryAlgorithm schedule)
    : IInitialization
{
    public Task<IIndividualList>? Process(Task<IIndividualList> individuals,
        IFitnessFunction fitnessFunction)
    {
        var searchSpace = schedule.SearchSpace;
        var population = searchSpace.GetAStartingPopulation();
        return Task.FromResult(population);
    }
}