using Italbytz.EA.Mutation;
using Italbytz.EA.Operator;
using Italbytz.EA.Selection;

namespace Italbytz.EA.Graph.Common;

public class OnePlusOneEAGraph : OperatorGraph
{
    public OnePlusOneEAGraph()
    {
        Start = new Start();
        Finish = new Finish();
        var mutation = new StandardMutation();
        var selection = new CutSelection();
        Start.AddChildren(mutation, selection);
        mutation.AddChildren(selection);
        selection.AddChildren(Finish);
    }
}