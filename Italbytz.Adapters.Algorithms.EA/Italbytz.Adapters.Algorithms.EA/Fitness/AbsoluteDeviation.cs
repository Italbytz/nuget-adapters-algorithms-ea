using System;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Fitness;

public class AbsoluteDeviation : IStaticSingleObjectiveFitnessFunction
{
    private readonly float[][] _features;
    private readonly float[] _labels;

    public AbsoluteDeviation(float[][] features, float[] labels)
    {
        _features = features;
        _labels = labels;
    }

    public int NumberOfObjectives { get; } = 1;

    double[] IFitnessFunction.Evaluate(IIndividual individual)
    {
        if (individual.Genotype is not IPredictingGenotype<int> genotype)
            throw new ArgumentException(
                "Expected genotype of type IPredictingGenotype");

        double totalAbsoluteDeviation = 0;
        var featuresLength = _features.Length;

        for (var i = 0; i < featuresLength; i++)
        {
            var prediction = genotype.PredictValue(_features[i]);
            totalAbsoluteDeviation += Math.Abs(prediction - _labels[i]);
        }

        // Since lower absolute deviation is better, we return its negative
        return [-totalAbsoluteDeviation];
    }
}