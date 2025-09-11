using System;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Fitness;

public class LogicGpPareto : IStaticMultiObjectiveFitnessFunction
{
    private readonly double[][] _features;
    private readonly string[] _labels;

    public LogicGpPareto(double[][] features, string[] labels)
    {
        _features = features;
        _labels = labels;
    }

    public double[] Evaluate(IIndividual individual)
    {
        throw new NotImplementedException();
    }

    public int NumberOfObjectives { get; }
}