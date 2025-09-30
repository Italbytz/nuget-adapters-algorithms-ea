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
        // Initialize confusion matrix
        var confusionTableCounts =
            new int[NumberOfObjectives, NumberOfObjectives];

        var size = individual.Size;
        if (size > MaxSize)
        {
            // Generate bad confusion matrix for oversized individuals
            for (var i = 0; i < NumberOfObjectives; i++)
            for (var j = 0; j < NumberOfObjectives; j++)
                if (i == j) confusionTableCounts[i, j] = 0;
                else confusionTableCounts[i, j] = 1;

            return new ConfusionAndSizeFitnessValue(
                new ConfusionMatrix(confusionTableCounts), size);
        }

        var featuresLength = _features.Length;

        // Predict classes for all features
        var predictions = genotype.PredictClasses(_features, _labels);

        // Use Span for faster iteration if possible (C# 7.2+), otherwise keep as is
        for (var i = 0; i < featuresLength; i++)
            // Avoid bounds checks by trusting input validity
            confusionTableCounts[_labels[i], predictions[i]]++;

        var confusionMatrix = new ConfusionMatrix(confusionTableCounts);


        return new ConfusionAndSizeFitnessValue(confusionMatrix, size);
    }
}