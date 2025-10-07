using Italbytz.EA.Crossover;
using Italbytz.EA.Graph;
using Italbytz.EA.Mutation;
using Italbytz.EA.Operator;
using Italbytz.EA.Selection;

namespace Italbytz.EA.Trainer.Bioinformatics;

public class LogicGpGraph : OperatorGraph
{
    public LogicGpGraph(int maxIndividuals = 1000, double crossoverRatio = 1.8,
        double mutationRatio = 0.1)
    {
        Start = new Start();
        var selectionForCrossover = new FitnessTournamentSelection
        {
            UseRatio = true,
            RatioOfIndividualsToSelect = crossoverRatio
        };
        Start.AddChildren(selectionForCrossover);
        var crossover = new StandardCrossover();
        var selectionsForMutation = new FitnessTournamentSelection[5];
        for (var i = 0; i < selectionsForMutation.Length; i++)
        {
            selectionsForMutation[i] = new FitnessTournamentSelection
            {
                UseRatio = true,
                RatioOfIndividualsToSelect = mutationRatio
            };
            Start.AddChildren(selectionsForMutation[i]);
        }

/*        var finalSelection = new DominationTournamentSelection
        {
            NoOfIndividualsToSelect = maxIndividuals
        };*/

        var finalSelection = new FitnessTournamentSelection
        {
            NoOfIndividualsToSelect = maxIndividuals
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
        finalSelection.AddChildren(Finish);
    }
}