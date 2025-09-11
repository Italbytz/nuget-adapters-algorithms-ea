using System.Linq;
using System.Threading.Tasks;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Selection;

namespace Italbytz.EA.GP.Selection;

/// <inheritdoc cref="ISelection" />
public class ParetoFrontSelection : ISelection
{
    public int Size { get; set; }

    /// <inheritdoc />
    public Task<IIndividualList>? Process(Task<IIndividualList> individuals,
        IFitnessFunction fitnessFunction)
    {
        var individualList = individuals.Result.ToList();
        var maxGeneration =
            individualList.Max(individual => individual.Generation);
        var i = 0;
        while (i < individualList.Count)
        {
            var individual = individualList[i];
            if (individual.Generation < maxGeneration) break;
            var j = i + 1;
            while (j < individualList.Count)
            {
                var otherIndividual = individualList[j];
                if (individual.IsDominating(otherIndividual))
                {
                    individualList.RemoveAt(j);
                }
                else if (otherIndividual.IsDominating(individual))
                {
                    individualList.RemoveAt(i);
                    i--;
                    break;
                }
                else
                {
                    j++;
                }
            }

            i++;
        }

        var population = new ListBasedPopulation();
        foreach (var individual in individualList) population.Add(individual);
        return Task.FromResult<IIndividualList>(population);
    }
}