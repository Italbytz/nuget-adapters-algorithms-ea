using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class LogicGpPolynomial<TCategory> : IPolynomial<TCategory>
{
    public LogicGpPolynomial(List<IMonomial<TCategory>> monomials)
    {
        Monomials = monomials;
    }

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
}