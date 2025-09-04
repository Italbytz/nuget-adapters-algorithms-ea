namespace Italbytz.EA.Operator;

public class Start : GraphOperator
{
    public override int MaxParents { get; } = 0;
    public override int MaxChildren { get; } = int.MaxValue;
}