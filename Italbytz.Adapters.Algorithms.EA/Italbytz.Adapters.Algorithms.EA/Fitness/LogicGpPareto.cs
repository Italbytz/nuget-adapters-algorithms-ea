using System;
using System.Linq;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Fitness;

public class LogicGpPareto<TCategory> : IStaticMultiObjectiveFitnessFunction
    where TCategory : notnull
{
    private readonly TCategory[][] _features;
    private readonly int[] _labels;

    public LogicGpPareto(TCategory[][] features, int[] labels)
    {
        _features = features;
        _labels = labels;
        NumberOfObjectives = labels.Max() + 2;
    }

    public int MaxSize { get; set; } = int.MaxValue;

    public double[] Evaluate(IIndividual individual)
    {
        if (individual.Genotype is not IPredictingGenotype<TCategory> genotype)
            throw new ArgumentException(
                "Expected genotype of type IPredictingGenotype");
        var size = individual.Size;
        if (size > MaxSize)
            return Enumerable.Repeat(double.NegativeInfinity,
                NumberOfObjectives).ToArray();
        var featuresLength = _features.Length;
        var objectives = new double[NumberOfObjectives];
        var missedObjectives = new int[NumberOfObjectives - 1];

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

        for (var i = 0; i < NumberOfObjectives - 1; i++)
            if (objectives[i] + missedObjectives[i] > 0)
                objectives[i] /= objectives[i] + missedObjectives[i];


        objectives[^1] = -size;

        return objectives;
    }

    public int NumberOfObjectives { get; }
}