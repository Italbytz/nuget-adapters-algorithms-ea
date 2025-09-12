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
        var crossover = new LogicGpCrossover();
        var selectionsForMutation = new UniformSelection[5];
        for (var i = 0; i < selectionsForMutation.Length; i++)
        {
            selectionsForMutation[i] = new UniformSelection
            {
                NoOfIndividualsToSelect = 1
            };
            Start.AddChildren(selectionsForMutation[i]);
        }

        var finalSelection = new DropTournamentWorst
        {
            NoOfIndividualsToSelect = 10000
        };

        var deleteLiteralMutation = new DeleteLiteral();
        selectionsForMutation[0].AddChildren(deleteLiteralMutation);
        deleteLiteralMutation.AddChildren(finalSelection);
        var deleteMonomialMutation = new DeleteMonomial();
        selectionsForMutation[1].AddChildren(deleteMonomialMutation);
        deleteMonomialMutation.AddChildren(finalSelection);
        var insertLiteralMutation = new InsertLiteral();
        selectionsForMutation[2].AddChildren(insertLiteralMutation);
        insertLiteralMutation.AddChildren(finalSelection);
        var insertMonomialMutation = new InsertMonomial();
        selectionsForMutation[3].AddChildren(insertMonomialMutation);
        insertMonomialMutation.AddChildren(finalSelection);
        var replaceLiteralMutation = new ReplaceLiteral();
        selectionsForMutation[4].AddChildren(replaceLiteralMutation);
        replaceLiteralMutation.AddChildren(finalSelection);
        selectionForCrossover.AddChildren(crossover);
        crossover.AddChildren(finalSelection);
        Start.AddChildren(finalSelection);
        Finish = new Finish();
    }
}