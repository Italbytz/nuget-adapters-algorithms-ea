using System;
using System.Collections.Generic;
using Italbytz.AI;
using Italbytz.EA.Individuals;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class LogicGpGenotype<TCategory> : IPredictingGenotype<TCategory>
{
    private readonly IPolynomial<TCategory> _polynomial;

    private LogicGpGenotype(IPolynomial<TCategory> polynomial)
    {
        _polynomial = polynomial;
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