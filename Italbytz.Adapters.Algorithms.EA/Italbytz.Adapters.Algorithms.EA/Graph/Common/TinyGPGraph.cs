using Italbytz.EA.Crossover;
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
        var finalSelection = new DropTournamentWorstSelection
        {
            NoOfIndividualsToSelect = 10000
        };
        Start.AddChildren(finalSelection);
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
        mutation.AddChildren(finalSelection);
        var crossoverSelection = new TournamentSelection
        {
            UseRatio = true,
            RatioOfIndividualsToSelect = 1.8 // 2 parents for 90 % of population
        };
        Start.AddChildren(crossoverSelection);
        var crossover = new TinyGpCrossover();
        crossoverSelection.AddChildren(crossover);
        crossover.AddChildren(finalSelection);
        finalSelection.AddChildren(Finish);
    }
}