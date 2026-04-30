using Microsoft.ML.Data;

namespace Italbytz.Adapters.Algorithms.EA.Tests;

internal static class MulticlassMetricsExtensions
{
    public static double F1Macro(this MulticlassClassificationMetrics metrics)
    {
        var confusion = metrics.ConfusionMatrix;
        if (confusion?.Counts is null)
        {
            return metrics.MacroAccuracy;
        }

        var counts = confusion.Counts;
        if (counts.Count == 0)
        {
            return 0d;
        }

        double sumF1 = 0d;
        int classesWithSupport = 0;

        for (int i = 0; i < counts.Count; i++)
        {
            var tp = counts[i][i];
            double fp = 0d;
            double fn = 0d;

            for (int row = 0; row < counts.Count; row++)
            {
                if (row != i)
                {
                    fp += counts[row][i];
                }
            }

            for (int col = 0; col < counts[i].Count; col++)
            {
                if (col != i)
                {
                    fn += counts[i][col];
                }
            }

            var support = tp + fn;
            if (support <= 0d)
            {
                continue;
            }

            var precisionDenominator = tp + fp;
            var recallDenominator = tp + fn;
            var precision = precisionDenominator > 0d ? tp / precisionDenominator : 0d;
            var recall = recallDenominator > 0d ? tp / recallDenominator : 0d;
            var f1Denominator = precision + recall;
            var f1 = f1Denominator > 0d ? 2d * precision * recall / f1Denominator : 0d;

            sumF1 += f1;
            classesWithSupport++;
        }

        return classesWithSupport > 0 ? sumF1 / classesWithSupport : 0d;
    }

    public static double F1ScoreBinary(this MulticlassClassificationMetrics metrics)
    {
        var confusion = metrics.ConfusionMatrix;
        if (confusion?.Counts is null || confusion.Counts.Count < 2)
        {
            return metrics.MacroAccuracy;
        }

        var counts = confusion.Counts;
        var positiveClass = 1;
        var tp = counts[positiveClass][positiveClass];
        double fp = 0d;
        double fn = 0d;

        for (int row = 0; row < counts.Count; row++)
        {
            if (row != positiveClass)
            {
                fp += counts[row][positiveClass];
            }
        }

        for (int col = 0; col < counts[positiveClass].Count; col++)
        {
            if (col != positiveClass)
            {
                fn += counts[positiveClass][col];
            }
        }

        var precisionDenominator = tp + fp;
        var recallDenominator = tp + fn;
        var precision = precisionDenominator > 0d ? tp / precisionDenominator : 0d;
        var recall = recallDenominator > 0d ? tp / recallDenominator : 0d;
        var f1Denominator = precision + recall;

        return f1Denominator > 0d ? 2d * precision * recall / f1Denominator : 0d;
    }
}
