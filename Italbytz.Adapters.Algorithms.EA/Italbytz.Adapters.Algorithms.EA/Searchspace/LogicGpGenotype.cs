using System;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Searchspace;

public class LogicGpGenotype<TCategory> : IPredictingGenotype<TCategory>
{
    public object Clone()
    {
        throw new NotImplementedException();
    }

    public double[]? LatestKnownFitness { get; set; }
    public int Size { get; }

    public double PredictValue(float[] features)
    {
        throw new NotImplementedException();
    }

    public string PredictClass(TCategory[] features)
    {
        throw new NotImplementedException();
    }
}