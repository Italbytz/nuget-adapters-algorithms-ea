using System;
using System.Collections.Generic;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class LogicGpMonomial<TCategory> : IMonomial<TCategory>
{
    public LogicGpMonomial(List<ILiteral<TCategory>> literals)
    {
        Literals = literals;
    }

    public object Clone()
    {
        throw new NotImplementedException();
    }

    public void RandomizeWeights(bool restricted)
    {
        throw new NotImplementedException();
    }

    public void UpdatePredictions()
    {
        throw new NotImplementedException();
    }

    public List<ILiteral<TCategory>> Literals { get; set; }
    public float[] Weights { get; set; }
    public float[][] Predictions { get; set; }
    public int Size { get; }
}