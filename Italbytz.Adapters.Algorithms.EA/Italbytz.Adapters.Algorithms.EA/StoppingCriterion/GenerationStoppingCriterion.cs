namespace Italbytz.EA.StoppingCriterion;

/// <inheritdoc cref="IStoppingCriterion" />
public class GenerationStoppingCriterion(IGenerationProvider generationProvider)
    : IStoppingCriterion
{
    public int Limit { get; set; } = 100;

    /// <inheritdoc />
    public bool IsMet()
    {
        return generationProvider.Generation >= Limit;
    }
}