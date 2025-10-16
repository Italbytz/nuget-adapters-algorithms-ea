using System;
using System.Collections.Generic;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class UniquenessSelection : AbstractSelection
{
    public override bool FitnessBasedSelection { get; } = true;

    public override object Clone()
    {
        throw new NotImplementedException();
    }

    protected override IEnumerable<IIndividual> Select(
        IIndividualList individualList, int noOfIndividualsToSelect)
    {
        var selectedIndividuals = new List<IIndividual>();
        var hashSet = new HashSet<string>();
        foreach (var individual in individualList)
        {
            var repr =
                $"{individual.Size}_{individual.LatestKnownFitness.ConsolidatedValue}";
            if (hashSet.Contains(repr)) continue;
            selectedIndividuals.Add(individual);
            hashSet.Add(repr);
            if (selectedIndividuals.Count >= noOfIndividualsToSelect)
                break;
        }

        return selectedIndividuals;
    }
}