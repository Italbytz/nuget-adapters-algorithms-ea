using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;

namespace Italbytz.EA.GP.Initialization;

/// <inheritdoc cref="IInitialization" />
public class RandomInitialization(IGeneticProgram gp) : IInitialization
{
    public int Size { get; set; }

    /// <inheritdoc />
    public Task<IIndividualList>? Process(Task<IIndividualList> individuals,
        IFitnessFunction fitnessFunction)
    {
        var result = new Population();
        var searchSpace = gp.SearchSpace;
        for (var i = 0; i < Size; i++)
            result
                .Add(new Individual(searchSpace.GetRandomGenotype(),
                    null));
        return Task.FromResult<IIndividualList>(result);
    }
}