using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.ML;

namespace Italbytz.EA.Fitness;

/// <summary>
///     Represents the
///     <a href="https://en.wikipedia.org/wiki/Confusion_matrix">confusion matrix</a>
///     of the classification results.
/// </summary>
public sealed class ConfusionMatrix : ICloneable
{
    /// <summary>
    ///     The indicators of the predicted classes.
    ///     It might be the classes names, or just indices of the predicted classes, if
    ///     the name mapping is missing.
    /// </summary>
    internal IReadOnlyList<ReadOnlyMemory<char>> PredictedClassesIndicators;

    /// <summary>
    ///     The confusion matrix as a structured type, built from the counts of the
    ///     confusion table <see cref="IDataView" /> that the
    ///     <see cref="BinaryClassifierEvaluator" /> or
    ///     the <see cref="MulticlassClassificationEvaluator" /> constructor.
    /// </summary>
    /// <param name="host">The IHost instance. </param>
    /// <param name="precision">The values of precision per class.</param>
    /// <param name="recall">The vales of recall per class.</param>
    /// <param name="confusionTableCounts">
    ///     The counts of the confusion table. The actual classes values are in the
    ///     rows of the 2D array,
    ///     and the counts of the predicted classes are in the columns.
    /// </param>
    /// <param name="labelNames">
    ///     The predicted classes names, or the indexes of the
    ///     classes, if the names are missing.
    /// </param>
    /// <param name="isSampled">Whether the classes are sampled.</param>
    /// <param name="isBinary">
    ///     Whether the confusion table is the result of a binary
    ///     classification.
    /// </param>
    public ConfusionMatrix(double[] precision, double[] recall,
        int[,] confusionTableCounts)
    {
        PerClassPrecision = precision.ToImmutableArray();
        PerClassRecall = recall.ToImmutableArray();
        Counts = confusionTableCounts;

        NumberOfClasses = confusionTableCounts.Length;
    }

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

    /// <summary>
    ///     The indicators of the predicted classes.
    ///     It might be the classes names, or just indices of the predicted classes, if
    ///     the name mapping is missing.
    /// </summary>
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

    public double GetMetric(Metric usedMetric)
    {
        return usedMetric switch
        {
            Metric.F1Score => ComputeF1Macro(),
            Metric.Precision => PerClassPrecision.Average(),
            Metric.Recall => PerClassRecall.Average(),
            _ => throw new ArgumentOutOfRangeException(nameof(usedMetric),
                usedMetric, null)
        };
    }

    private double ComputeF1Macro()
    {
        double f1Sum = 0;
        var classCount = NumberOfClasses;

        for (var i = 0; i < classCount; i++)
        {
            var dividend = PerClassPrecision[i] *
                           PerClassRecall[i];
            var divisor = PerClassPrecision[i] +
                          PerClassRecall[i];
            if (divisor == 0) continue;
            // Calculate F1 score for class i
            f1Sum += 2 * (dividend /
                          divisor);
        }

        return f1Sum / classCount;
    }
}