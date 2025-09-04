using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;

namespace Italbytz.EA.GP.Initialization;

/// <inheritdoc cref="IInitialization" />
public class CompleteInitialization(IGeneticProgram gp) : IInitialization
{
    public int Size { get; set; }

    /// <inheritdoc />
    public Task<IIndividualList>? Process(Task<IIndividualList> individuals,
        IFitnessFunction fitnessFunction)
    {
        var searchSpace = gp.SearchSpace;
        var population = searchSpace.GetAStartingPopulation();
        return Task.FromResult(population);
    }
}