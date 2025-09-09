using System.Collections.Generic;
using System.Linq;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class CutSelection : AbstractSelection
{
    protected override IEnumerable<IIndividual> Select(
        IIndividualList individualList)
    {
        return individualList
            .OrderByDescending(i => i.LatestKnownFitness.Sum())
            .Take(NoOfIndividualsToSelect);
    }
}