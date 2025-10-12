using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Italbytz.EA.Fitness;

/// <summary>
///     Represents the
///     <a href="https://en.wikipedia.org/wiki/Confusion_matrix">confusion matrix</a>
///     of the classification results.
/// </summary>
public sealed class ConfusionMatrix : ICloneable
{
    /// <summary>
    ///     The confusion matrix as a structured type, built from the counts of the
    ///     confusion table .
    /// </summary>
    /// <param name="confusionTableCounts">
    ///     The counts of the confusion table. The actual classes values are in the
    ///     rows of the 2D array,
    ///     and the counts of the predicted classes are in the columns.
    /// </param>
    public ConfusionMatrix(
        int[,] confusionTableCounts)
    {
        NumberOfClasses = confusionTableCounts.GetLength(0);
        // Calculate precision and recall per class
        var precisionPerClass = new double[NumberOfClasses];
        var recallPerClass = new double[NumberOfClasses];
        for (var i = 0; i < NumberOfClasses; i++)
        {
            var tp = confusionTableCounts[i, i];
            var fp = 0;
            var fn = 0;
            // Unroll loop for small NumberOfObjectives for better performance
            for (var j = 0; j < NumberOfClasses; j++)
                if (j != i)
                {
                    // Predicted as i, but actually j
                    fp += confusionTableCounts[j, i];
                    // Actually i, but predicted as j
                    fn += confusionTableCounts[i, j];
                }

            precisionPerClass[i] = tp + fp > 0 ? (double)tp / (tp + fp) : 0.0;
            recallPerClass[i] = tp + fn > 0 ? (double)tp / (tp + fn) : 0.0;
        }

        PerClassPrecision = precisionPerClass.ToImmutableArray();
        PerClassRecall = recallPerClass.ToImmutableArray();
        Counts = confusionTableCounts;
        Accuracy = (double)Enumerable.Range(0, NumberOfClasses)
                       .Select(t => confusionTableCounts[t, t]).Sum() /
                   confusionTableCounts.Cast<int>().Sum();
    }

    internal ConfusionMatrix(double[] precision, double[] recall,
        int[,] confusionTableCounts)
    {
        PerClassPrecision = precision.ToImmutableArray();
        PerClassRecall = recall.ToImmutableArray();
        Counts = confusionTableCounts;

        NumberOfClasses = precision.Length;
    }

    public double Accuracy { get; set; }

    /// <summary>
    ///     The calculated value of
    ///     <a href="https://en.wikipedia.org/wiki/Precision_and_recall#Precision">precision</a>
    ///     for each class.
    /// </summary>
    public IReadOnlyList<double> PerClassPrecision { get; }

    /// <summary>
    ///     The calculated value of
    ///     <a href="https://en.wikipedia.org/wiki/Precision_and_recall#Recall">recall</a>
    ///     for each class.
    /// </summary>
    public IReadOnlyList<double> PerClassRecall { get; }

    /// <summary>
    ///     The confusion matrix counts for the combinations actual class/predicted
    ///     class.
    ///     The actual classes are in the rows of the table (stored in the outer
    ///     <see cref="IReadOnlyList{T}" />), and the predicted classes
    ///     in the columns(stored in the inner <see cref="IReadOnlyList{T}" />).
    /// </summary>
    public int[,] Counts { get; }

    public int NumberOfClasses { get; }

    public object Clone()
    {
        return new ConfusionMatrix(PerClassPrecision.ToArray(),
            PerClassRecall.ToArray(), (int[,])Counts.Clone());
    }


    /// <summary>
    ///     Gets the confusion table count for the pair
    ///     <paramref name="predictedClassIndicatorIndex" />/
    ///     <paramref name="actualClassIndicatorIndex" />.
    /// </summary>
    /// <param name="predictedClassIndicatorIndex">
    ///     The index of the predicted label
    ///     indicator, in the <see cref="PredictedClassesIndicators" />.
    /// </param>
    /// <param name="actualClassIndicatorIndex">
    ///     The index of the actual label
    ///     indicator, in the <see cref="PredictedClassesIndicators" />.
    /// </param>
    /// <returns></returns>
    public int GetCountForClassPair(int predictedClassIndicatorIndex,
        int actualClassIndicatorIndex)
    {
        return Counts[actualClassIndicatorIndex, predictedClassIndicatorIndex];
    }

    public double[] GetPerClassMetric(ClassMetric usedMetric, bool compress)
    {
        var perClassMetric = usedMetric switch
        {
            ClassMetric.F1 => ComputePerClassF1Score(),
            ClassMetric.Precision => PerClassPrecision.ToArray(),
            ClassMetric.Recall => PerClassRecall.ToArray(),
            ClassMetric.Accuracy => PerClassRecall.ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(usedMetric),
                usedMetric, null)
        };
        if (!compress) return perClassMetric;
        var maxIndex = Array.IndexOf(perClassMetric,
            perClassMetric.Max());
        var avgNonClassMetrics =
            perClassMetric.Where((t, i) => i != maxIndex).Average();
        return [perClassMetric[maxIndex], avgNonClassMetrics];
    }

    private double[] ComputeMaxClassRecallAvgNonClassRecall()
    {
        var maxIndex = Array.IndexOf(PerClassRecall.ToArray(),
            PerClassRecall.Max());
        var avgNonClassAccuracies =
            PerClassRecall.Where((t, i) => i != maxIndex).Average();
        return [PerClassRecall[maxIndex], avgNonClassAccuracies];
    }


    private double[] ComputePerClassF1Score()
    {
        var f1Scores = new double[NumberOfClasses];
        for (var i = 0; i < NumberOfClasses; i++)
        {
            var dividend = PerClassPrecision[i] *
                           PerClassRecall[i];
            var divisor = PerClassPrecision[i] +
                          PerClassRecall[i];
            if (divisor == 0)
                f1Scores[i] = 0;
            else
                // Calculate F1 score for class i
                f1Scores[i] = 2 * (dividend /
                                   divisor);
        }

        return f1Scores;
    }
}