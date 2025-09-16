using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class CutSelection : AbstractSelection
{
    public override bool FitnessBasedSelection { get; } = true;

    protected override IEnumerable<IIndividual> Select(
        IIndividualList individualList, int noOfIndividualsToSelect)
    {
        return individualList
            .OrderByDescending(i => i.LatestKnownFitness.Sum())
            .Take(noOfIndividualsToSelect);
    }

    public override object Clone()
    {
        throw new NotImplementedException();
    }
}