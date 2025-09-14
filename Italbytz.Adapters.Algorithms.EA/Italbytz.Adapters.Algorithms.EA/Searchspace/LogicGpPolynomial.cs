using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Italbytz.AI;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class LogicGpPolynomial<TCategory> : IPolynomial<TCategory>
{
    public LogicGpPolynomial(List<IMonomial<TCategory>> monomials)
    {
        Monomials = monomials;
    }

    public float[] Weights { get; set; } = [1.0f, 1.0f, 0.0f];

    public int Size
    {
        get { return Monomials.Sum(monomial => monomial.Size); }
    }

    public object Clone()
    {
        var monomials =
            Monomials.Select(monomial =>
                (IMonomial<TCategory>)monomial.Clone());
        return new LogicGpPolynomial<TCategory>(monomials.ToList());
    }

    public float[] Evaluate(TCategory[] input)
    {
        var result = new[] { 0.0f, 0.0f, 0.0f };
        foreach (var monomial in Monomials)
        {
            var monomialResult = monomial.Evaluate(input);
            for (var i = 0; i < result.Length; i++)
                result[i] += monomialResult[i];
        }

        return result;
    }

    public IMonomial<TCategory> GetRandomMonomial()
    {
        var random = ThreadSafeRandomNetCore.Shared;
        return Monomials[random.Next(Monomials.Count)];
    }

    public void UpdatePredictions()
    {
        throw new NotImplementedException();
    }

    public List<ILiteral<TCategory>> GetAllLiterals()
    {
        throw new NotImplementedException();
    }

    public List<IMonomial<TCategory>> Monomials { get; set; }
    public float[][] Predictions { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("\n|");
        for (var i = 0; i < Weights.Length; i++) sb.Append($" $w_{i}$ |");
        sb.Append(" Condition                                   |\n|");
        for (var i = 0; i < Weights.Length; i++) sb.Append(" ----- |");
        sb.Append(" ------------------------------------------- |\n|  ");
        sb.Append(string.Join(" |  ", Weights.Select(w => w.ToString("F2",
            CultureInfo.InvariantCulture))));
        sb.Append(" | None below fulfilled                        |\n|");
        sb.Append(string.Join("\n|", Monomials));
        sb.Append('\n');
        return sb.ToString();
    }
}