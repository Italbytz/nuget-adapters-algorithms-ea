using System;
using Italbytz.EA.Individuals;
using Italbytz.EA.Searchspace;

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
        if (individual.Genotype is not TinyGpGenotype genotype)
            throw new ArgumentException(
                "Expected genotype of type TinyGpGenotype");

        var totalAbsoluteDeviation = 0.0;

        //Console.WriteLine($"Evaluating individual: {genotype}");

        for (var i = 0; i < _features.Length; i++)
        {
            var pc = 0;
            var prediction = genotype.Run(_features[i], ref pc);


            totalAbsoluteDeviation += Math.Abs(prediction - _labels[i]);
        }

        // Since lower absolute deviation is better, we return its negative
        return [-totalAbsoluteDeviation];
    }
}