using Italbytz.EA.Crossover;
using Italbytz.EA.Mutation;
using Italbytz.EA.Operator;
using Italbytz.EA.Selection;

namespace Italbytz.EA.Graph.Common;

public class LogicGPGeccoGraph : OperatorGraph
{
    public LogicGPGeccoGraph()
    {
        Start = new Start();
        var selectionForCrossover = new UniformSelection
        {
            NoOfIndividualsToSelect = 2
        };
        Start.AddChildren(selectionForCrossover);
        var crossover = new TinyGpCrossover();
        var selectionsForMutation = new UniformSelection[5];
        for (var i = 0; i < selectionsForMutation.Length; i++)
        {
            selectionsForMutation[i] = new UniformSelection
            {
                NoOfIndividualsToSelect = 1
            };
            Start.AddChildren(selectionsForMutation[i]);
        }

        var mutations = new StandardMutation[5];
        for (var i = 0; i < mutations.Length; i++)
        {
            mutations[i] = new StandardMutation
            {
                MutationProbability = 0.1
            };
            selectionsForMutation[i].AddChildren(mutations[i]);
        }

        selectionForCrossover.AddChildren(crossover);
        var finalSelection = new DropTournamentWorst
        {
            NoOfIndividualsToSelect = 10000
        };
        crossover.AddChildren(finalSelection);
        Start.AddChildren(finalSelection);
        for (var i = 0; i < mutations.Length; i++)
            mutations[i].AddChildren(finalSelection);
        Finish = new Finish();
    }
}