using System;
using System.Linq;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Fitness;

public class ConfusionAndSizeFitnessFunction<TCategory> : IFitnessFunction
    where TCategory : notnull
{
    private readonly TCategory[][] _features;
    private readonly int[] _labels;

    public ConfusionAndSizeFitnessFunction(TCategory[][] features, int[] labels)
    {
        _features = features;
        _labels = labels;
        NumberOfObjectives = labels.Max() + 1;
    }

    public int MaxSize { get; set; } = int.MaxValue;

    public int NumberOfObjectives { get; }

    public IFitnessValue Evaluate(IIndividual individual)
    {
        if (individual.Genotype is not IPredictingGenotype<TCategory> genotype)
            throw new ArgumentException(
                "Expected genotype of type IPredictingGenotype");
        var size = individual.Size;
        if (size > MaxSize)
            return new ConfusionAndSizeFitnessValue(null, size);
        var featuresLength = _features.Length;

        // Predict classes for all features
        var predictions = genotype.PredictClasses(_features, _labels);

        // Initialize confusion matrix
        var confusionTableCounts =
            new int[NumberOfObjectives, NumberOfObjectives];

        // Use Span for faster iteration if possible (C# 7.2+), otherwise keep as is
        for (var i = 0; i < featuresLength; i++)
            // Avoid bounds checks by trusting input validity
            confusionTableCounts[_labels[i], predictions[i]]++;

        // Calculate precision and recall per class
        var precisionPerClass = new double[NumberOfObjectives];
        var recallPerClass = new double[NumberOfObjectives];
        for (var i = 0; i < NumberOfObjectives; i++)
        {
            var tp = confusionTableCounts[i, i];
            var fp = 0;
            var fn = 0;
            // Unroll loop for small NumberOfObjectives for better performance
            for (var j = 0; j < NumberOfObjectives; j++)
                if (j != i)
                {
                    fp += confusionTableCounts[j, i];
                    fn += confusionTableCounts[i, j];
                }

            precisionPerClass[i] = tp + fp > 0 ? (double)tp / (tp + fp) : 0.0;
            recallPerClass[i] = tp + fn > 0 ? (double)tp / (tp + fn) : 0.0;
        }

        var confusionMatrix = new ConfusionMatrix(precisionPerClass,
            recallPerClass, confusionTableCounts);


        return new ConfusionAndSizeFitnessValue(confusionMatrix, size);
    }
}