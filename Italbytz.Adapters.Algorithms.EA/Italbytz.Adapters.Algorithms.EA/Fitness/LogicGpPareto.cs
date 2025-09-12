using System;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Fitness;

public class LogicGpPareto<TCategory> : IStaticMultiObjectiveFitnessFunction
{
    private readonly TCategory[][] _features;
    private readonly TCategory[] _labels;

    public LogicGpPareto(TCategory[][] features, TCategory[] labels)
    {
        _features = features;
        _labels = labels;
    }

    public double[] Evaluate(IIndividual individual)
    {
        if (individual.Genotype is not IPredictingGenotype<TCategory> genotype)
            throw new ArgumentException(
                "Expected genotype of type IPredictingGenotype");
        var featuresLength = _features.Length;
        for (var i = 0; i < featuresLength; i++)
        {
            var prediction = genotype.PredictClass(_features[i]);
        }

        // ToDo: Do we need information about the labels here or can we assume a certain label format?
        // ToDo: Implement logic after this is clear
        return [0.0];
    }

    public int NumberOfObjectives { get; }
}