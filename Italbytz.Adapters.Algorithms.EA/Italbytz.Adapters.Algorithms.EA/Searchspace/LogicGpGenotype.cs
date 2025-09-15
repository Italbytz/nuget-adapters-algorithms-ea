using System;
using System.Collections.Generic;
using System.Linq;
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

    public Weighting Weighting { get; set; } = Weighting.Fixed;

    public PredictionStrategy PredictionStrategy { get; set; } =
        PredictionStrategy.Max;

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

    public int PredictClass(TCategory[] features)
    {
        var result = _polynomial.Evaluate(features);
        var chosenIndex = PredictionStrategy switch
        {
            PredictionStrategy.Max => MaxIndex(result),
            PredictionStrategy.SoftmaxProbability =>
                SoftmaxProbabilityIndex(result),
            _ => throw new ArgumentOutOfRangeException()
        };
        return chosenIndex;
    }

    public int[] PredictClasses(TCategory[][] features, int[] labels)
    {
        if (Weighting != Weighting.Fixed) ComputeWeights(features, labels);
        var results = new int[features.Length];
        for (var i = 0; i < features.Length; i++)
            results[i] = PredictClass(features[i]);
        return results;
    }

    private void ComputeWeights(TCategory[][] features, int[] labels)
    {
        var classes = labels.Max() + 1;
        if (_polynomial.Monomials.Count == 0) return;
        //var predictions = new bool[features.Length][];
        var counts = new int[_polynomial.Monomials.Count + 1][];
        var counterCounts = new int[_polynomial.Monomials.Count + 1][];
        for (var i = 0; i < features.Length; i++)
        {
            //predictions[i] = new bool[_polynomial.Monomials.Count + 1];
            var allFalse = true;
            for (var j = 0; j < _polynomial.Monomials.Count; j++)
            {
                if (counts[j] == null)
                    counts[j] = new int[classes];
                if (counterCounts[j] == null)
                    counterCounts[j] = new int[classes];
                var monomial = _polynomial.Monomials[j];
                var prediction = monomial.EvaluateLiterals(features[i]);
                if (prediction)
                {
                    counts[j][labels[i]]++;
                    //predictions[i][j] = true;
                    allFalse = false;
                }
                else
                {
                    counterCounts[j][labels[i]]++;
                }
            }

            if (counts[^1] == null)
                counts[^1] = new int[classes];
            if (counterCounts[^1] == null)
                counterCounts[^1] = new int[classes];
            if (allFalse)
                counts[^1][labels[i]]++;
            //predictions[i][^1] = true;
            else
                counterCounts[^1][labels[i]]++;
        }

        for (var i = 0; i < _polynomial.Monomials.Count; i++)
        {
            var monomial = _polynomial.Monomials[i];
            var count = counts[i];
            var counterCount = counterCounts[i];
            monomial.Weights = ComputeDistributionWeights(count, counterCount);
        }

        _polynomial.Weights =
            ComputeDistributionWeights(counts[^1], counterCounts[^1]);
    }

    private float[] ComputeDistributionWeights(int[] count, int[] counterCount)
    {
        var inDistribution = count.Select(c => (float)c).ToArray();
        var outDistribution = counterCount.Select(c => (float)c).ToArray();
        var sum = inDistribution.Sum();
        if (sum == 0)
            sum = 1;
        for (var j = 0; j < inDistribution.Length; j++)
            inDistribution[j] /= sum;

        sum = outDistribution.Sum();
        if (sum == 0)
            sum = 1;
        for (var j = 0; j < outDistribution.Length; j++)
            outDistribution[j] /= sum;

        var newWeights = new float[count.Length];
        for (var j = 0; j < newWeights.Length; j++)
            if (outDistribution[j] == 0)
                newWeights[j] = inDistribution[j];
            else
                newWeights[j] = inDistribution[j] / outDistribution[j];

        return newWeights;
    }

    private int SoftmaxProbabilityIndex(float[] result)
    {
        var exp = new float[result.Length];
        var sum = 0.0f;
        for (var i = 0; i < result.Length; i++)
        {
            exp[i] = (float)Math.Exp(result[i]);
            sum += exp[i];
        }

        var probabilities = new float[result.Length];
        for (var i = 0; i < result.Length; i++)
            probabilities[i] = exp[i] / sum;

        var randomValue = (float)ThreadSafeRandomNetCore.Shared.NextDouble();
        var cumulativeProbability = 0.0f;
        for (var i = 0; i < probabilities.Length; i++)
        {
            cumulativeProbability += probabilities[i];
            if (randomValue <= cumulativeProbability)
                return i;
        }

        return probabilities.Length - 1;
    }

    private int MaxIndex(float[] result)
    {
        var maxIndex = 0;
        for (var i = 1; i < result.Length; i++)
            if (result[i] > result[maxIndex])
                maxIndex = i;
        return maxIndex;
    }


    public override string ToString()
    {
        return _polynomial.ToString() ?? string.Empty;
    }

    private IMonomial<TCategory> GetRandomMonomial()
    {
        return _polynomial.GetRandomMonomial();
    }

    public static IGenotype GenerateRandomGenotype<TCategory>(
        List<ILiteral<TCategory>> literals, Weighting weighting)
    {
        var literal =
            literals[ThreadSafeRandomNetCore.Shared.Next(literals.Count)];
        var monomial = new LogicGpMonomial<TCategory>([literal]);
        var polynomial = new LogicGpPolynomial<TCategory>([monomial]);
        return new LogicGpGenotype<TCategory>(polynomial, literals)
        {
            Weighting = weighting
        };
    }
}