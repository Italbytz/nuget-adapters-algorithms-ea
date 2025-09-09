namespace Italbytz.EA.Operator;

public class Finish : GraphOperator
{
    public override int MaxChildren { get; } = 0;
    public override int MaxParents { get; } = int.MaxValue;
}