using System;
using System.Globalization;
using System.Linq;

namespace Italbytz.EA.Fitness;

public class ConfusionAndSizeFitnessValue(ConfusionMatrix? matrix, int size)
    : IFitnessValue
{
    public static Metric UsedMetric { get; set; } = Metric.F1Score;

    public double[] Objectives { get; init; } =
    {
        matrix?.GetMetric(UsedMetric) ?? 0.0
    };

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

        var atLeastOneBetter = false;
        for (var i = 0; i < Objectives.Length; i++)
        {
            if (Objectives[i] < other.Objectives[i])
                return false; // This fitness is worse in at least one objective
            if (Objectives[i] > other.Objectives[i])
                atLeastOneBetter =
                    true; // This fitness is better in at least one objective
        }

        // If all objectives are equal, compare sizes (smaller size is better)
        return atLeastOneBetter || Size <= other.Size;
    }

    public double ConsolidatedValue => Objectives.Sum();

    private int Compare(ConfusionAndSizeFitnessValue fitnessValue,
        IFitnessValue? other)
    {
        if (other is null) return 1;
        if (other is not ConfusionAndSizeFitnessValue otherFitnessValue)
            return -1;

        // First, compare based on objectives
        var objectivesComparison = fitnessValue.Objectives.Sum()
            .CompareTo(otherFitnessValue.Objectives.Sum());
        return objectivesComparison != 0
            ? objectivesComparison
            : otherFitnessValue.Size.CompareTo(fitnessValue.Size);
    }

    public override string ToString()
    {
        return
            $"[{string.Join(", ", Objectives.Select(o => o.ToString(CultureInfo.InvariantCulture)))}], Size: {Size}";
    }
}