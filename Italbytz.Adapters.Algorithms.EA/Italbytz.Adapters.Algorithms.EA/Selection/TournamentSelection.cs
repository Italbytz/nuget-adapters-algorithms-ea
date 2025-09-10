using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class TournamentSelection : AbstractSelection
{
    public int TournamentSize { get; set; } = 2;

    protected override IEnumerable<IIndividual> Select(
        IIndividualList individualList, int noOfIndividualsToSelect)
    {
        var selectedIndividuals = new IIndividual[noOfIndividualsToSelect];
        var rnd = ThreadSafeRandomNetCore.LocalRandom;
        for (var i = 0; i < noOfIndividualsToSelect; i++)
        {
            IIndividual? fittest = null;
            for (var j = 0; j < TournamentSize; j++)
            {
                var individual =
                    individualList[rnd.Next(individualList.Count())];
                if (fittest == null ||
                    individual.LatestKnownFitness.Sum() >
                    fittest.LatestKnownFitness.Sum())
                    fittest = individual;
            }

            selectedIndividuals[i] = fittest;
        }

        return selectedIndividuals;
    }
}