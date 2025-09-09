using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class TournamentSelection : AbstractSelection
{
    public int TournamentSize { get; set; } = 2;
    protected bool SelectWorst { get; set; } = false;

    protected override IEnumerable<IIndividual> Select(
        IIndividualList individualList)
    {
        var selectedIndividuals = new List<IIndividual>();
        var rnd = ThreadSafeRandomNetCore.LocalRandom;
        for (var i = 0; i < NoOfIndividualsToSelect; i++)
        {
            var tournament = new List<IIndividual>();
            for (var j = 0; j < TournamentSize; j++)
            {
                var individual =
                    individualList[rnd.Next(individualList.Count())];
                tournament.Add(individual);
            }

            tournament.Sort((a, b) =>
                b.LatestKnownFitness.Sum()
                    .CompareTo(a.LatestKnownFitness.Sum()));
            selectedIndividuals.Add(SelectWorst
                ? tournament.Last()
                : tournament.First());
        }

        return selectedIndividuals;
    }
}