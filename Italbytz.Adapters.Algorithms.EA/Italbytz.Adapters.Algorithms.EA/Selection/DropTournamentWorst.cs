using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class DropTournamentWorst : AbstractSelection
{
    public int TournamentSize { get; set; } = 2;

    public override bool FitnessBasedSelection { get; } = true;

    protected override IEnumerable<IIndividual> Select(
        IIndividualList individualList, int noOfIndividualsToSelect)
    {
        var count = individualList.Count;
        if (noOfIndividualsToSelect >= count) return individualList;

        var rnd = ThreadSafeRandomNetCore.Shared;
        var result = new List<IIndividual>(noOfIndividualsToSelect);
        var selected = new bool[count];
        var noOfIndividualsToDrop = count - noOfIndividualsToSelect;
        var dropped = 0;

        while (dropped < noOfIndividualsToDrop)
        {
            IIndividual? unfittest = null;
            var unfittestIndex = -1;

            // Select distinct indices for the tournament
            var tournamentIndices = new HashSet<int>();
            while (tournamentIndices.Count < TournamentSize)
            {
                var candidateIndex = rnd.Next(count);
                if (!selected[candidateIndex] &&
                    !tournamentIndices.Contains(candidateIndex))
                    tournamentIndices.Add(candidateIndex);
            }

            foreach (var selectedIndex in tournamentIndices)
            {
                var individual = individualList[selectedIndex];

                if (unfittest == null || individual.LatestKnownFitness.CompareTo(unfittest.LatestKnownFitness) < 0)
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

    public override object Clone()
    {
        throw new NotImplementedException();
    }
}