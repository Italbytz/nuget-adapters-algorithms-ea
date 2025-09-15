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

    public double[] Evaluate(IIndividual individual)
    {
        if (individual.Genotype is not IPredictingGenotype<TCategory> genotype)
            throw new ArgumentException(
                "Expected genotype of type IPredictingGenotype");

        var featuresLength = _features.Length;
        var objectives = new double[NumberOfObjectives];

        var predictions = genotype.PredictClasses(_features, _labels);

        for (var i = 0; i < featuresLength; i++)
            if (predictions[i] == _labels[i])
                objectives[_labels[i]]++;

        objectives[^1] = -individual.Size;
        individual.LatestKnownFitness = objectives;

        return objectives;
    }

    public int NumberOfObjectives { get; }
}