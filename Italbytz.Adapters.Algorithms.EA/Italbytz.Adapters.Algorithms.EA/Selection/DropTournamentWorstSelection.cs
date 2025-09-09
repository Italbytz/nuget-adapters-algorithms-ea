using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class DropTournamentWorstSelection : AbstractSelection
{
    public int TournamentSize { get; set; } = 2;

    protected override IEnumerable<IIndividual> Select(
        IIndividualList individualList, int noOfIndividualsToSelect)
    {
        var noOfIndividualsToDrop =
            individualList.Count() - noOfIndividualsToSelect;
        if (noOfIndividualsToDrop <= 0) return individualList;
        var selectedIndividuals = individualList.ToList();
        var rnd = ThreadSafeRandomNetCore.LocalRandom;
        for (var i = 0; i < noOfIndividualsToDrop; i++)
        {
            var tournament = new List<IIndividual>();
            for (var j = 0;
                 j < Math.Min(TournamentSize, selectedIndividuals.Count);
                 j++)
            {
                var individual =
                    selectedIndividuals[rnd.Next(selectedIndividuals.Count)];
                tournament.Add(individual);
            }

            tournament.Sort((a, b) =>
                b.LatestKnownFitness.Sum()
                    .CompareTo(a.LatestKnownFitness.Sum()));
            selectedIndividuals.Remove(tournament.Last());
        }

        foreach (var ind in selectedIndividuals
                     .OrderByDescending(i => i.LatestKnownFitness.Sum())
                     .Take(1))
            Console.WriteLine(
                $"Genotype {ind.Genotype} Fitness: {ind.LatestKnownFitness.Sum()}");
        return selectedIndividuals;
    }
}