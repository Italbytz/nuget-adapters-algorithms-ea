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

        NumberOfClasses = precision.Length;
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

    public double[] GetPerClassMetric(Metric usedMetric)
    {
        return usedMetric switch
        {
            Metric.F1Score => ComputePerClassF1Score(),
            Metric.Precision => PerClassPrecision.ToArray(),
            Metric.Recall => PerClassRecall.ToArray(),
            Metric.MicroAccuracy => ComputePerClassMicroAccuracy(),
            Metric.MacroAccuracy => ComputePerClassMacroAccuracy(),
            _ => throw new ArgumentOutOfRangeException(nameof(usedMetric),
                usedMetric, null)
        };
    }

    private double[] ComputePerClassMacroAccuracy()
    {
        var accuracies = new double[NumberOfClasses];
        for (var i = 0; i < NumberOfClasses; i++)
        {
            var truePositives = Counts[i, i];
            var falseNegatives = 0;
            var falsePositives = 0;
            for (var j = 0; j < NumberOfClasses; j++)
                if (j != i)
                {
                    falseNegatives += Counts[i, j];
                    falsePositives += Counts[j, i];
                }

            var total = truePositives + falseNegatives + falsePositives;
            accuracies[i] = total == 0 ? 0 : truePositives / total;
        }

        return accuracies;
    }

    private double[] ComputePerClassMicroAccuracy()
    {
        var accuracies = new double[NumberOfClasses];
        for (var i = 0; i < NumberOfClasses; i++) accuracies[i] = Counts[i, i];
        return accuracies;
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