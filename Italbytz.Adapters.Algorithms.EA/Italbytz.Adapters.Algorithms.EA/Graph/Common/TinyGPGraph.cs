using Italbytz.EA.Mutation;
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
        Start.AddChildren(mutationSelection);
        var mutation = new StandardMutation
        {
            MutationProbability = 0.02
        };
        mutationSelection.AddChildren(mutation);
        mutation.AddChildren(Finish);
        var crossoverSelection = new TournamentSelection
        {
            UseRatio = true,
            RatioOfIndividualsToSelect = 1.8 // 2 parents for 90 % of population
        };

        Start.AddChildren(crossoverSelection);
        crossoverSelection.AddChildren(Finish);
    }
}