using System.Collections.Generic;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Selection;

public class UniformSelection : AbstractSelection
{
    protected override IEnumerable<IIndividual> Select(
        IIndividualList individualList, int noOfIndividualsToSelect)
    {
        var result = new ListBasedPopulation();
        for (var i = 0; i < noOfIndividualsToSelect; i++)
            result.Add(individualList.GetRandomIndividual());
        return result;
    }
}