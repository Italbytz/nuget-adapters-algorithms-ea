using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
        var counterWeights = new float[Weights.Length];
        return EvaluateLiterals(input) ? Weights : counterWeights;
    }

    public bool EvaluateLiterals(TCategory[] input)
    {
        foreach (var literal in Literals)
            if (!literal.Evaluate(input))
                return false;

        return true;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("  ");
        sb.Append(string.Join(" |  ", Weights.Select(w => w.ToString("F2",
            CultureInfo.InvariantCulture))));
        sb.Append(" | ");
        sb.Append(string.Join("", Literals));
        sb.Append(" |");
        return sb.ToString();
    }
}