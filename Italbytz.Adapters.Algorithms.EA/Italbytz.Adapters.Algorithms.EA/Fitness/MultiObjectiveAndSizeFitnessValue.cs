using System;
using System.Linq;

namespace Italbytz.EA.Fitness;

public class MultiObjectiveAndSizeFitnessValue(double[] objectives, int size) : IFitnessValue
{
    public int CompareTo(IFitnessValue? other)
    {
        return Compare(this, other);
    }

    private int Compare(MultiObjectiveAndSizeFitnessValue fitnessValue, IFitnessValue? other)
    {
        if (other is null) return 1;
        if (other is not MultiObjectiveAndSizeFitnessValue otherFitnessValue) return -1;

        // First, compare based on objectives
        var objectivesComparison = fitnessValue.Objectives.Sum().CompareTo(otherFitnessValue.Objectives.Sum());
        return objectivesComparison != 0 ? objectivesComparison :
            otherFitnessValue.Size.CompareTo(fitnessValue.Size);
    }

    public object Clone()
    {
        return new MultiObjectiveAndSizeFitnessValue(objectives.ToArray(), size);
    }

    public bool IsDominating(IFitnessValue otherFitnessValue)
    {
        if (otherFitnessValue is not MultiObjectiveAndSizeFitnessValue other)
            throw new ArgumentException("Expected fitness value of type MultiObjectiveAndSizeFitnessValue");

        var atLeastOneBetter = false;
        for (var i = 0; i < Objectives.Length; i++)
        {
            if (Objectives[i] < other.Objectives[i])
                return false; // This fitness is worse in at least one objective
            if (Objectives[i] > other.Objectives[i])
                atLeastOneBetter = true; // This fitness is better in at least one objective
        }

        // If all objectives are equal, compare sizes (smaller size is better)
        return atLeastOneBetter || Size < other.Size;
    }

    public double ConsolidatedValue => Objectives.Sum();

    public double[] Objectives { get; } = objectives;
    public int Size { get; } = size;
    
    public override string ToString()
    {
        return $"[{string.Join(", ", Objectives.Select(o => o.ToString(System.Globalization.CultureInfo.InvariantCulture)))}], Size: {Size}";
    }
}