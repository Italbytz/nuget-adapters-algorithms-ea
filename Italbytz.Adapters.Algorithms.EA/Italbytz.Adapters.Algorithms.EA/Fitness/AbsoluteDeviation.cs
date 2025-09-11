using System;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Fitness;

public class AbsoluteDeviation : IStaticSingleObjectiveFitnessFunction
{
    private readonly double[][] _features;
    private readonly double[] _labels;

    public AbsoluteDeviation(double[][] features, double[] labels)
    {
        _features = features;
        _labels = labels;
    }

    public int NumberOfObjectives { get; } = 1;

    double[] IFitnessFunction.Evaluate(IIndividual individual)
    {
        if (individual.Genotype is not IPredictingGenotype genotype)
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