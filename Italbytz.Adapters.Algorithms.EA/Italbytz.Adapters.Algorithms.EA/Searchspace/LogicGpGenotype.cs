using System;
using System.Collections.Generic;
using Italbytz.AI;
using Italbytz.EA.Crossover;
using Italbytz.EA.Individuals;
using Italbytz.EA.Mutation;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class LogicGpGenotype<TCategory> : IPredictingGenotype<TCategory>,
    ILogicGpMutable, ILogicGpCrossable
{
    private readonly IPolynomial<TCategory> _polynomial;

    private LogicGpGenotype(IPolynomial<TCategory> polynomial)
    {
        _polynomial = polynomial;
    }

    public void CrossWith(ILogicGpCrossable parentGenotype)
    {
        throw new NotImplementedException();
    }

    public void DeleteRandomLiteral()
    {
        throw new NotImplementedException();
    }

    public bool IsEmpty()
    {
        return _polynomial.Monomials.Count == 0;
    }

    public void DeleteRandomMonomial()
    {
        throw new NotImplementedException();
    }

    public void InsertRandomLiteral()
    {
        throw new NotImplementedException();
    }

    public void InsertRandomMonomial()
    {
        throw new NotImplementedException();
    }

    public void ReplaceRandomLiteral()
    {
        throw new NotImplementedException();
    }

    public object Clone()
    {
        throw new NotImplementedException();
    }

    public double[]? LatestKnownFitness { get; set; }
    public int Size => _polynomial.Size;

    public double PredictValue(float[] features)
    {
        throw new NotImplementedException();
    }

    public string PredictClass(TCategory[] features)
    {
        var result = _polynomial.Evaluate(features);
        var maxIndex = 0;
        for (var i = 1; i < result.Length; i++)
            if (result[i] > result[maxIndex])
                maxIndex = i;
        maxIndex++;
        return maxIndex.ToString();
    }

    public static IGenotype GenerateRandomGenotype<TCategory>(
        List<ILiteral<TCategory>> literals)
    {
        var literal =
            literals[ThreadSafeRandomNetCore.Shared.Next(literals.Count)];
        var monomial = new LogicGpMonomial<TCategory>([literal]);
        var polynomial = new LogicGpPolynomial<TCategory>([monomial]);
        return new LogicGpGenotype<TCategory>(polynomial);
    }
}