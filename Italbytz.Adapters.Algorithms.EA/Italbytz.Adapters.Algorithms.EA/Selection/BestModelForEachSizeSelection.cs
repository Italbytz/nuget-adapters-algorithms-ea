using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class BestModelForEachSizeSelection : AbstractSelection
{
    public override bool FitnessBasedSelection { get; } = true;

    protected override IEnumerable<IIndividual> Select(
        IIndividualList individualList, int noOfIndividualsToSelect)
    {
        var population = new ListBasedPopulation();

        var groupedIndividuals =
            individualList.GroupBy(individual => individual.Size);
        foreach (var group in groupedIndividuals)
        {
            var bestIndividual = group.First();
            var bestFitness = 0.0;
            foreach (var individual in group)
            {
                var fitness =
                    (individual.LatestKnownFitness ?? Array.Empty<double>())
                    .Aggregate(0.0, (current, fitval) => (fitval >= 0.0 ? current + fitval: current));
                if (fitness <= bestFitness) continue;
                bestFitness = fitness;
                bestIndividual = individual;
            }

            population.Add(bestIndividual);
        }

        return population;
    }

    public override object Clone()
    {
        throw new NotImplementedException();
    }
}