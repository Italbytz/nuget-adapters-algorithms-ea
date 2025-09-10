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
        var count = individualList.Count;
        if (noOfIndividualsToSelect >= count) return individualList;

        var rnd = ThreadSafeRandomNetCore.LocalRandom;
        var result = new List<IIndividual>(noOfIndividualsToSelect);
        var selected = new bool[count];
        var noOfIndividualsToDrop = count - noOfIndividualsToSelect;
        var dropped = 0;

        while (dropped < noOfIndividualsToDrop)
        {
            IIndividual? unfittest = null;
            var unfittestIndex = -1;

            for (var j = 0; j < TournamentSize; j++)
            {
                var selectedIndex = rnd.Next(count);
                var individual = individualList[selectedIndex];

                if (unfittest == null || individual.LatestKnownFitness.Sum() <
                    unfittest.LatestKnownFitness.Sum())
                {
                    unfittest = individual;
                    unfittestIndex = selectedIndex;
                }
            }

            if (unfittestIndex != -1 && !selected[unfittestIndex])
            {
                selected[unfittestIndex] = true;
                dropped++;
            }
        }

        for (var i = 0; i < count; i++)
            if (!selected[i])
                result.Add(individualList[i]);

        return result;
    }
}