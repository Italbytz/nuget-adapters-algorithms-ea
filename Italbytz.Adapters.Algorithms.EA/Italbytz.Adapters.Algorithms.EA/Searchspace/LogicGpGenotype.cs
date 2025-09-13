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
    private readonly List<ILiteral<TCategory>> _literals;
    private readonly IPolynomial<TCategory> _polynomial;

    private LogicGpGenotype(IPolynomial<TCategory> polynomial,
        List<ILiteral<TCategory>> literals)
    {
        _polynomial = polynomial;
        _literals = literals;
    }

    public void CrossWith(ILogicGpCrossable parentGenotype)
    {
        if (parentGenotype is not LogicGpGenotype<TCategory> parent)
            throw new InvalidOperationException(
                "Parent genotype is not of the same type");
        var monomial =
            (IMonomial<TCategory>)parent._polynomial
                .GetRandomMonomial().Clone();
        _polynomial.Monomials.Add(monomial);
        LatestKnownFitness = null;
    }

    public void DeleteRandomLiteral()
    {
        var monomial = GetRandomMonomial();
        if (monomial.Literals.Count == 0)
        {
            _polynomial.Monomials.Remove(monomial);
            return;
        }

        monomial.Literals.RemoveAt(
            ThreadSafeRandomNetCore.Shared.Next(monomial.Literals.Count));
        if (monomial.Literals.Count == 0)
            _polynomial.Monomials.Remove(monomial);
        LatestKnownFitness = null;
    }

    public bool IsEmpty()
    {
        return _polynomial.Monomials.Count == 0;
    }

    public void DeleteRandomMonomial()
    {
        _polynomial.Monomials.RemoveAt(
            ThreadSafeRandomNetCore.Shared.Next(
                _polynomial.Monomials.Count));
        LatestKnownFitness = null;
    }

    public void InsertRandomLiteral()
    {
        var monomial = GetRandomMonomial();
        monomial.Literals.Add(_literals[
            ThreadSafeRandomNetCore.Shared.Next(_literals.Count)]);
        LatestKnownFitness = null;
    }

    public void InsertRandomMonomial()
    {
        var literal =
            _literals[ThreadSafeRandomNetCore.Shared.Next(_literals.Count)];
        var monomial = new LogicGpMonomial<TCategory>([literal]);
        _polynomial.Monomials.Add(monomial);
        LatestKnownFitness = null;
    }

    public void ReplaceRandomLiteral()
    {
        var monomial = GetRandomMonomial();
        if (monomial.Literals.Count == 0)
        {
            _polynomial.Monomials.Remove(monomial);
            InsertRandomMonomial();
            return;
        }

        var literalIndex =
            ThreadSafeRandomNetCore.Shared.Next(monomial.Literals.Count);
        monomial.Literals[literalIndex] =
            _literals[ThreadSafeRandomNetCore.Shared.Next(_literals.Count)];
        LatestKnownFitness = null;
    }

    public object Clone()
    {
        var clonedPolynomial =
            (IPolynomial<TCategory>)_polynomial.Clone();
        return new LogicGpGenotype<TCategory>(clonedPolynomial, _literals);
    }

    public double[]? LatestKnownFitness { get; set; }
    public int Size => _polynomial.Size;

    public float PredictValue(float[] features)
    {
        throw new NotImplementedException();
    }

    public float[] PredictValues(float[][] features, float[] labels)
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

    public string[] PredictClass(TCategory[][] features, string[] labels)
    {
        throw new NotImplementedException();
    }

    private IMonomial<TCategory> GetRandomMonomial()
    {
        return _polynomial.GetRandomMonomial();
    }

    public static IGenotype GenerateRandomGenotype<TCategory>(
        List<ILiteral<TCategory>> literals)
    {
        var literal =
            literals[ThreadSafeRandomNetCore.Shared.Next(literals.Count)];
        var monomial = new LogicGpMonomial<TCategory>([literal]);
        var polynomial = new LogicGpPolynomial<TCategory>([monomial]);
        return new LogicGpGenotype<TCategory>(polynomial, literals);
    }
}