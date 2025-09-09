using Italbytz.EA.Operator;
using Italbytz.EA.Selection;

namespace Italbytz.EA.Graph.Common;

public class TinyGPGraph : OperatorGraph
{
    public TinyGPGraph()
    {
        Start = new Start();
        Finish = new Finish();
        var mutationSelection = new TournamentSelection
        {
            UseRatio = true,
            RatioOfIndividualsToSelect = 0.1
        };
        var crossoverSelection = new TournamentSelection
        {
            UseRatio = true,
            RatioOfIndividualsToSelect = 0.9
        };
        Start.AddChildren(mutationSelection);
        Start.AddChildren(crossoverSelection);
        mutationSelection.AddChildren(Finish);
        crossoverSelection.AddChildren(Finish);
    }
}