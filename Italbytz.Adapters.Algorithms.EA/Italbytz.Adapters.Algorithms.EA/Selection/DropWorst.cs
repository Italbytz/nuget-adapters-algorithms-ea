using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class DropWorst : AbstractSelection
{
    public override bool FitnessBasedSelection { get; } = true;

    protected override IEnumerable<IIndividual> Select(
        IIndividualList individualList, int noOfIndividualsToSelect)
    {
        var noOfIndividualsToDrop =
            individualList.Count - noOfIndividualsToSelect;
        if (noOfIndividualsToDrop <= 0) return individualList;
        var best = individualList
            .OrderByDescending(i => i.LatestKnownFitness)
            .Take(individualList.Count - noOfIndividualsToDrop);
        /*Console.WriteLine(
            $"Genotype {best.First().Genotype} Fitness: {best.First().Genotype.LatestKnownFitness}");*/
        return best;
    }

    public override object Clone()
    {
        throw new NotImplementedException();
    }
}