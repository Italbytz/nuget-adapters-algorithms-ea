using Italbytz.EA.Crossover;
using Italbytz.EA.Graph;
using Italbytz.EA.Mutation;
using Italbytz.EA.Operator;
using Italbytz.EA.Selection;

namespace Italbytz.EA.Trainer.Bioinformatics;

public class LogicGpGraph : OperatorGraph
{
    public LogicGpGraph(int maxIndividuals = 1000,
        int crossoverIndividuals = 2, int mutationIndividuals = 1)
    {
        Start = new Start();
        var selectionForCrossover = new UniformSelection
        {
            NoOfIndividualsToSelect = crossoverIndividuals
        };
        Start.AddChildren(selectionForCrossover);
        var crossover = new StandardCrossover();
        var selectionsForMutation = new UniformSelection[5];
        for (var i = 0; i < selectionsForMutation.Length; i++)
        {
            selectionsForMutation[i] = new UniformSelection
            {
                NoOfIndividualsToSelect = mutationIndividuals
            };
            Start.AddChildren(selectionsForMutation[i]);
        }

        /*var finalSelection = new DropTournamentWorst
        {
            NoOfIndividualsToSelect = maxIndividuals
        };*/

        var uniquenessSelection = new UniquenessSelection
        {
            NoOfIndividualsToSelect = int.MaxValue
        };

        var finalSelection = new DominationTournamentSelection
        {
            NoOfIndividualsToSelect = maxIndividuals
        };

        /*var finalSelection = new ParetoFrontSelection
        {
            NoOfIndividualsToSelect = maxIndividuals
        };*/

        /*var finalSelection = new FitnessTournamentSelection
        {
            NoOfIndividualsToSelect = maxIndividuals
        };*/

        var deleteLiteralMutation = new DeleteLiteral();
        selectionsForMutation[0].AddChildren(deleteLiteralMutation);
        deleteLiteralMutation.AddChildren(uniquenessSelection);
        var deleteMonomialMutation = new DeleteMonomial();
        selectionsForMutation[1].AddChildren(deleteMonomialMutation);
        deleteMonomialMutation.AddChildren(uniquenessSelection);
        var insertLiteralMutation = new InsertLiteral();
        selectionsForMutation[2].AddChildren(insertLiteralMutation);
        insertLiteralMutation.AddChildren(uniquenessSelection);
        var insertMonomialMutation = new InsertMonomial();
        selectionsForMutation[3].AddChildren(insertMonomialMutation);
        insertMonomialMutation.AddChildren(uniquenessSelection);
        var replaceLiteralMutation = new ReplaceLiteral();
        selectionsForMutation[4].AddChildren(replaceLiteralMutation);
        replaceLiteralMutation.AddChildren(uniquenessSelection);
        selectionForCrossover.AddChildren(crossover);
        crossover.AddChildren(uniquenessSelection);
        Start.AddChildren(uniquenessSelection);
        uniquenessSelection.AddChildren(finalSelection);
        Finish = new Finish();
        finalSelection.AddChildren(Finish);
    }
}