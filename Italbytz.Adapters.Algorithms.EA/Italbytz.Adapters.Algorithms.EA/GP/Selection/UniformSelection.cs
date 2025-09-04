using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Selection;

namespace Italbytz.EA.GP.Selection;

/// <inheritdoc cref="ISelection" />
public class UniformSelection : ISelection
{
    /// <inheritdoc />
    public int Size { get; set; }

    /// <inheritdoc />
    public Task<IIndividualList>? Process(Task<IIndividualList> individuals,
        IFitnessFunction fitnessFunction)
    {
        var result = new Population();
        var population = individuals.Result;
        for (var i = 0; i < Size; i++)
            result.Add(population.GetRandomIndividual());
        return Task.FromResult<IIndividualList>(result);
    }
}