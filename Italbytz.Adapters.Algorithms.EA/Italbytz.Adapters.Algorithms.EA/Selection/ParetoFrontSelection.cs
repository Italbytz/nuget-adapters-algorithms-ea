using System.Collections.Generic;
using System.Linq;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class ParetoFrontSelection : AbstractSelection
{
    public override bool FitnessBasedSelection { get; } = true;

    protected override IEnumerable<IIndividual> Select(
        IIndividualList individualList, int noOfIndividualsToSelect)
    {
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
        return population;
    }
}