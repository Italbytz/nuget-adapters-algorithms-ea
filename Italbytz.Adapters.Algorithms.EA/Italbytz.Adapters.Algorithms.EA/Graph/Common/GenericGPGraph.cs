using System;
using System.Collections.Generic;
using Italbytz.EA.Operator;
using Italbytz.EA.Selection;

namespace Italbytz.EA.Graph.Common;

public class GenericGPGraph : OperatorGraph
{
    public GenericGPGraph(IGraphOperator selectionForOperator,
        List<IGraphOperator> mutations,
        List<IGraphOperator> crossovers
        , IGraphOperator selectionForSurvival)
    {
        Start = new Start();
        Finish = new Finish();
        Start.AddChildren(selectionForSurvival);
        selectionForSurvival.AddChildren(Finish);
        foreach (var mutation in mutations)
        {
            var selectionForMutation = selectionForOperator.Clone();
            if (selectionForMutation is not AbstractSelection selection)
                throw new Exception(
                    "Selection for operator must be of type AbstractSelection");
            selection.NoOfIndividualsToSelect = 1;
            Start.AddChildren(selection);
            selection.AddChildren(mutation);
            mutation.AddChildren(selectionForSurvival);
        }

        foreach (var crossover in crossovers)
        {
            var selectionForCrossover = selectionForOperator.Clone();
            if (selectionForCrossover is not AbstractSelection selection)
                throw new Exception(
                    "Selection for operator must be of type AbstractSelection");
            selection.NoOfIndividualsToSelect = 2;
            Start.AddChildren(selection);
            selection.AddChildren(crossover);
            crossover.AddChildren(selectionForSurvival);
        }
    }
}