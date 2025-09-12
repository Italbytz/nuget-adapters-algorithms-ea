using System;
using System.Collections.Generic;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class LogicGpPolynomial<TCategory> : IPolynomial<TCategory>
{
    public LogicGpPolynomial(List<IMonomial<TCategory>> monomials)
    {
        Monomials = monomials;
    }

    public object Clone()
    {
        throw new NotImplementedException();
    }

    public IMonomial<TCategory> GetRandomMonomial()
    {
        throw new NotImplementedException();
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
    public int Size { get; }
}