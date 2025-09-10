using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class DropTournamentWorst : AbstractSelection
{
    public int TournamentSize { get; set; } = 2;

    protected override IEnumerable<IIndividual> Select(
        IIndividualList individualList, int noOfIndividualsToSelect)
    {
        var noOfIndividualsToDrop =
            individualList.Count - noOfIndividualsToSelect;
        if (noOfIndividualsToDrop <= 0) return individualList;
        var rnd = ThreadSafeRandomNetCore.LocalRandom;
        var dropped = 0;
        SortedSet<int> droppedIndices = [];
        while (dropped < noOfIndividualsToDrop)
        {
            IIndividual? unfittest = null;
            var unfittestIndex = -1;
            for (var j = 0;
                 j < TournamentSize;
                 j++)
            {
                var selectedIndex = rnd.Next(individualList.Count);
                var individual =
                    individualList[selectedIndex];
                if (unfittest != null &&
                    !(individual.LatestKnownFitness.Sum() <
                      unfittest.LatestKnownFitness.Sum())) continue;
                unfittest = individual;
                unfittestIndex = selectedIndex;
            }

            if (unfittestIndex != -1 &&
                droppedIndices.Add(unfittestIndex))
                dropped++;
        }

        var selectedIndividuals = individualList
            .Where((ind, index) => !droppedIndices.Contains(index));

        return selectedIndividuals;
    }
}