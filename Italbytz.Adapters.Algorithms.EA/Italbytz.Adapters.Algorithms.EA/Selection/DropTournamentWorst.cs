using System;
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
        var selectedIndividuals = individualList.ToList();
        var rnd = ThreadSafeRandomNetCore.LocalRandom;
        for (var i = 0; i < noOfIndividualsToDrop; i++)
        {
            IIndividual? unfittest = null;
            for (var j = 0;
                 j < Math.Min(TournamentSize, selectedIndividuals.Count);
                 j++)
            {
                var individual =
                    selectedIndividuals[rnd.Next(selectedIndividuals.Count)];
                if (unfittest == null ||
                    individual.LatestKnownFitness.Sum() <
                    unfittest.LatestKnownFitness.Sum())
                    unfittest = individual;
            }

            // ToDo: This is a big performance issue that needs to be fixed
            selectedIndividuals.Remove(unfittest);
        }

        /*foreach (var ind in selectedIndividuals
                     .OrderByDescending(i => i.LatestKnownFitness.Sum())
                     .Take(1))
            Console.WriteLine(
                $"Genotype {ind.Genotype} Fitness: {ind.LatestKnownFitness.Sum()}");*/
        return selectedIndividuals;
    }
}