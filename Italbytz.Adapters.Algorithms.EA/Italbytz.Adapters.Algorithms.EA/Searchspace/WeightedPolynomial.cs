using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Italbytz.AI;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class
    WeightedPolynomial<TLiteral, TCategory> : IPolynomial<
    WeightedMonomial<TLiteral, TCategory>,
    TLiteral, TCategory>
    where TLiteral : ILiteral<TCategory>
{
    public WeightedPolynomial(
        IList<WeightedMonomial<TLiteral, TCategory>> monomials)
    {
        Monomials = monomials;
    }

    public IList<WeightedMonomial<TLiteral, TCategory>> Monomials { get; set; }

    public float[] Weights { get; set; }

    //public int Size => GetAllLiterals().Count;

    public int Size
    {
        get { return Monomials.Sum(monomial => monomial.Size); }
    }

    public object Clone()
    {
        var monomials =
            Monomials.Select(monomial =>
                (WeightedMonomial<TLiteral, TCategory>)monomial.Clone());
        return new WeightedPolynomial<TLiteral, TCategory>(
            monomials.ToList());
    }

    public float[] Evaluate(TCategory[] input)
    {
        // Early return if no monomials
        if (Monomials.Count == 0)
            return Weights;

        // Pre-allocate result array based on first monomial result
        var firstMonomialResult = Monomials[0].Evaluate(input);
        var resultLength = firstMonomialResult.Length;
        var result = new float[resultLength];

        // Add first result directly
        Array.Copy(firstMonomialResult, result, resultLength);

        // Process remaining monomials
        for (var m = 1; m < Monomials.Count; m++)
        {
            var monomialResult = Monomials[m].Evaluate(input);
            for (var i = 0; i < resultLength; i++)
                result[i] += monomialResult[i];
        }

        // Check if sum is zero and return weights instead
        return result.Sum() == 0.0f ? Weights : result;
    }

    public WeightedMonomial<TLiteral, TCategory> GetRandomMonomial()
    {
        var random = ThreadSafeRandomNetCore.Shared;
        return Monomials[random.Next(Monomials.Count)];
    }

    public IList<TLiteral> GetAllLiterals()
    {
        return Monomials.SelectMany(m => m.Literals).Distinct().ToList();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("\n|");
        if (Weights != null)
        {
            for (var i = 0; i < Weights.Length; i++) sb.Append($" $w_{i}$ |");
            sb.Append(" Condition                                   |\n|");
            for (var i = 0; i < Weights.Length; i++) sb.Append(" ----- |");
            sb.Append(" ------------------------------------------- |\n|  ");
            sb.Append(string.Join(" |  ", Weights.Select(w => w.ToString("F2",
                CultureInfo.InvariantCulture))));
            sb.Append(" | None below fulfilled                        |\n|");
        }

        sb.Append(string.Join("\n|", Monomials));
        sb.Append('\n');
        return sb.ToString();
    }
}