using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class WeightedMonomial<TCategory> : IMonomial<TCategory>
{
    public WeightedMonomial(List<ILiteral<TCategory>> literals)
    {
        Literals = literals;
    }

    public object Clone()
    {
        // Create a new list with cloned literals
        var literalsCopy = Literals.Select(l => l)
            .ToList();

        // Copy weights for better performance
        float[] weightsCopy = null;
        if (Weights != null)
        {
            weightsCopy = new float[Weights.Length];
            Array.Copy(Weights, weightsCopy, Weights.Length);
        }

        return new WeightedMonomial<TCategory>(literalsCopy)
        {
            Weights = weightsCopy
        };
    }

    public List<ILiteral<TCategory>> Literals { get; set; }
    public float[] Weights { get; set; }
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
        if (Weights != null)
        {
            sb.Append("  ");
            sb.Append(string.Join(" |  ", Weights.Select(w => w.ToString("F2",
                CultureInfo.InvariantCulture))));
        }

        sb.Append(" | ");
        sb.Append(string.Join("", Literals));
        sb.Append(" |");
        return sb.ToString();
    }
}