using System;
using System.Linq;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Fitness;

public class ClassPredictionsAndSize<TCategory> : IFitnessFunction
    where TCategory : notnull
{
    private readonly TCategory[][] _features;
    private readonly int[] _labels;

    public ClassPredictionsAndSize(TCategory[][] features, int[] labels)
    {
        _features = features;
        _labels = labels;
        NumberOfObjectives = labels.Max() + 1;
    }

    public int MaxSize { get; set; } = int.MaxValue;

    public IFitnessValue Evaluate(IIndividual individual)
    {
        if (individual.Genotype is not IPredictingGenotype<TCategory> genotype)
            throw new ArgumentException(
                "Expected genotype of type IPredictingGenotype");
        var size = individual.Size;
        if (size > MaxSize)
            return new MultiObjectiveAndSizeFitnessValue(Enumerable.Repeat(double.NegativeInfinity,
                NumberOfObjectives).ToArray(), size);
        var featuresLength = _features.Length;
        var objectives = new double[NumberOfObjectives];
        var missedObjectives = new int[NumberOfObjectives];

        var predictions = genotype.PredictClasses(_features, _labels);

        int label;
        for (var i = 0; i < featuresLength; i++)
        {
            label = _labels[i];
            if (predictions[i] == label)
                objectives[label]++;
            else
                missedObjectives[label]++;
        }

        for (var i = 0; i < NumberOfObjectives; i++)
            if (objectives[i] + missedObjectives[i] > 0)
                objectives[i] /= objectives[i] + missedObjectives[i];
        
        return new MultiObjectiveAndSizeFitnessValue(objectives,size);
    }

    public int NumberOfObjectives { get; }
}