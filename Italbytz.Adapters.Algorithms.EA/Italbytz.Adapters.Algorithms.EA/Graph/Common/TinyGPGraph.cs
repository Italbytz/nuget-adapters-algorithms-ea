using Italbytz.EA.Operator;

namespace Italbytz.EA.Graph.Common;

public class TinyGPGraph : OperatorGraph
{
    public TinyGPGraph()
    {
        Start = new Start();
        Finish = new Finish();
    }
}