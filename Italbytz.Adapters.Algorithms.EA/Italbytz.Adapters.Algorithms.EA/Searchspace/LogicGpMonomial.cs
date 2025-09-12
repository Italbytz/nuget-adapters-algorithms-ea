using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class LogicGpMonomial<TCategory> : IMonomial<TCategory>
{
    public LogicGpMonomial(List<ILiteral<TCategory>> literals)
    {
        Literals = literals;
    }

    public float[] CounterWeights { get; set; } = [0.0f, 0.0f, 1.0f];

    public object Clone()
    {
        return new LogicGpMonomial<TCategory>(Literals)
        {
            Weights = new float[Weights.Length].Select((_, i) => Weights[i])
                .ToArray()
        };
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
    public float[] Weights { get; set; } = [1.0f, 1.0f, 0.0f];
    public float[][] Predictions { get; set; }
    public int Size => Literals.Count;

    public float[] Evaluate(TCategory[] input)
    {
        var allLiteralsTrue = true;
        foreach (var literal in Literals)
        {
            var literalResult = literal.Evaluate(input);
            if (!literalResult)
            {
                allLiteralsTrue = false;
                break;
            }
        }

        return allLiteralsTrue ? Weights : CounterWeights;
    }
}