using System;
using System.Globalization;
using System.Linq;

namespace Italbytz.EA.Fitness;

public class ConfusionAndSizeFitnessValue(
    ConfusionMatrix? matrix,
    int size,
    bool compress = true)
    : IFitnessValue
{
    public static (ClassMetric, Averaging) UsedMetric { get; set; } =
        (ClassMetric.F1, Averaging.Macro);

    public double[] Objectives { get; init; } =
        matrix?.GetPerClassMetric(UsedMetric.Item1, compress) ?? [0.0];

    public int SpecializationClass { get; init; } =
        matrix?.PerClassRecall != null
            ? Array.IndexOf(matrix.PerClassRecall.ToArray(),
                matrix.PerClassRecall.Max())
            : -1;

    public ConfusionMatrix? Matrix { get; } = matrix;
    public int Size { get; } = size;

    public int CompareTo(IFitnessValue? other)
    {
        return Compare(this, other);
    }

    public object Clone()
    {
        return new ConfusionAndSizeFitnessValue(
            (ConfusionMatrix?)Matrix?.Clone(),
            size);
    }

    public bool IsDominating(IFitnessValue otherFitnessValue)
    {
        if (otherFitnessValue is not ConfusionAndSizeFitnessValue other)
            throw new ArgumentException(
                "Expected fitness value of type MultiObjectiveAndSizeFitnessValue");
        // Only dominate if specializing in the same class
        if (SpecializationClass != other.SpecializationClass)
            return false;
        if (Objectives.Where((t, i) => t < other.Objectives[i]).Any())
            return false;
        return Size <= other.Size;
    }

    public double ConsolidatedValue => UsedMetric.Item2 == Averaging.Macro
        ? Objectives.Average()
        : matrix?.Accuracy ?? 0.0;

    private int Compare(ConfusionAndSizeFitnessValue fitnessValue,
        IFitnessValue? other)
    {
        if (other is null) return 1;
        if (other is not ConfusionAndSizeFitnessValue otherFitnessValue)
            return -1;

        // First, compare based on objectives
        var objectivesComparison = fitnessValue.ConsolidatedValue
            .CompareTo(otherFitnessValue.ConsolidatedValue);
        return objectivesComparison != 0
            ? objectivesComparison
            : otherFitnessValue.Size.CompareTo(fitnessValue.Size);
    }

    public override string ToString()
    {
        return
            $"[{string.Join(", ", Objectives.Select(o => o.ToString(CultureInfo.InvariantCulture)))}] ({ConsolidatedValue.ToString(CultureInfo.InvariantCulture)}), Size: {Size}";
    }
}