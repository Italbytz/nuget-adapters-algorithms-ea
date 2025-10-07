namespace Italbytz.EA.StoppingCriterion;

/// <inheritdoc cref="IStoppingCriterion" />
public class AdaptionsStoppingCriterion(
    IAdaptionCountProvider adaptionsProvider)
    : IStoppingCriterion
{
    public int Limit { get; set; } = 100;

    /// <inheritdoc />
    public bool IsMet()
    {
        return adaptionsProvider.Adaptions >= Limit;
    }
}