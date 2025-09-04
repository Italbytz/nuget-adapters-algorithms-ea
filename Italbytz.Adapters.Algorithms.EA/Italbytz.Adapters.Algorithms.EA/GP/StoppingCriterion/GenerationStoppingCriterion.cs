using Italbytz.EA.StoppingCriterion;

namespace Italbytz.EA.GP.StoppingCriterion;

/// <inheritdoc cref="IStoppingCriterion" />
public class GenerationStoppingCriterion(IGeneticProgram gp)
    : IStoppingCriterion
{
    public int Limit { get; set; }

    /// <inheritdoc />
    public bool IsMet()
    {
        return gp.Generation >= Limit;
    }
}