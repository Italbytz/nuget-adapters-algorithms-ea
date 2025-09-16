using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Italbytz.AI;
using Italbytz.EA.Crossover;
using Italbytz.EA.Individuals;
using Italbytz.EA.Mutation;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Searchspace;

public class LogicGpGenotype<TCategory> : IPredictingGenotype<TCategory>,
    ILogicGpMutable, ILogicGpCrossable, IFreezable, IValidatableGenotype
{
    private readonly List<ILiteral<TCategory>> _literals;
    public readonly IPolynomial<TCategory> Polynomial;

    private LogicGpGenotype(IPolynomial<TCategory> polynomial,
        List<ILiteral<TCategory>> literals, Weighting weighting)
    {
        Polynomial = polynomial;
        _literals = literals;
        Weighting = weighting;
    }

    public Weighting Weighting { get; set; } = Weighting.Fixed;

    public PredictionStrategy PredictionStrategy { get; set; } =
        PredictionStrategy.Max;

    public void Freeze()
    {
        Weighting = Weighting.Fixed;
    }

    public void CrossWith(ILogicGpCrossable parentGenotype)
    {
        if (LatestKnownFitness != null) throw new InvalidOperationException();
        if (parentGenotype is not LogicGpGenotype<TCategory> parent)
            throw new InvalidOperationException(
                "Parent genotype is not of the same type");
        var monomial =
            (IMonomial<TCategory>)parent.Polynomial
                .GetRandomMonomial().Clone();
        Polynomial.Monomials.Add(monomial);
    }

    public void DeleteRandomLiteral()
    {
        if (LatestKnownFitness != null) throw new InvalidOperationException();
        var monomial = GetRandomMonomial();
        if (monomial.Literals.Count == 0)
        {
            Polynomial.Monomials.Remove(monomial);
            return;
        }

        monomial.Literals.RemoveAt(
            ThreadSafeRandomNetCore.Shared.Next(monomial.Literals.Count));
        if (monomial.Literals.Count == 0)
            Polynomial.Monomials.Remove(monomial);
    }

    public bool IsEmpty()
    {
        return Polynomial.Size == 0;
    }

    public void DeleteRandomMonomial()
    {
        if (LatestKnownFitness != null) throw new InvalidOperationException();
        Polynomial.Monomials.RemoveAt(
            ThreadSafeRandomNetCore.Shared.Next(
                Polynomial.Monomials.Count));
    }

    public void InsertRandomLiteral()
    {
        if (LatestKnownFitness != null) throw new InvalidOperationException();
        var monomial = GetRandomMonomial();
        monomial.Literals.Add(_literals[
            ThreadSafeRandomNetCore.Shared.Next(_literals.Count)]);
    }

    public void InsertRandomMonomial()
    {
        if (LatestKnownFitness != null) throw new InvalidOperationException();
        var literal =
            _literals[ThreadSafeRandomNetCore.Shared.Next(_literals.Count)];
        var monomial = new LogicGpMonomial<TCategory>([literal]);
        Polynomial.Monomials.Add(monomial);
    }

    public void ReplaceRandomLiteral()
    {
        if (LatestKnownFitness != null) throw new InvalidOperationException();
        var monomial = GetRandomMonomial();
        if (monomial.Literals.Count == 0)
        {
            Polynomial.Monomials.Remove(monomial);
            InsertRandomMonomial();
            return;
        }

        var literalIndex =
            ThreadSafeRandomNetCore.Shared.Next(monomial.Literals.Count);
        monomial.Literals[literalIndex] =
            _literals[ThreadSafeRandomNetCore.Shared.Next(_literals.Count)];
    }

    public object Clone()
    {
        var clonedPolynomial =
            (IPolynomial<TCategory>)Polynomial.Clone();
        return new LogicGpGenotype<TCategory>(clonedPolynomial, _literals,
            Weighting);
    }

    public double[]? LatestKnownFitness { get; set; }
    public int Size => Polynomial.Size;

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
        var result = Polynomial.Evaluate(features);
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
        Parallel.For(0, features.Length,
            i => { results[i] = PredictClass(features[i]); });
        return results;
    }

    public double[]? TrainingFitness { get; set; }
    public double[]? ValidationFitness { get; set; }

    private void ComputeWeights(TCategory[][] features, int[] labels)
    {
        var classes = labels.Max() + 1;
        if (Polynomial.Monomials.Count == 0) return;
        var counts = new int[Polynomial.Monomials.Count + 1][];
        var counterCounts = new int[Polynomial.Monomials.Count + 1][];
        for (var i = 0; i < features.Length; i++)
        {
            var allFalse = true;
            for (var j = 0; j < Polynomial.Monomials.Count; j++)
            {
                if (counts[j] == null)
                    counts[j] = new int[classes];
                if (counterCounts[j] == null)
                    counterCounts[j] = new int[classes];
                var monomial = Polynomial.Monomials[j];
                var prediction = monomial.EvaluateLiterals(features[i]);
                if (prediction)
                {
                    counts[j][labels[i]]++;
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
            else
                counterCounts[^1][labels[i]]++;
        }

        for (var i = 0; i < Polynomial.Monomials.Count; i++)
        {
            var monomial = Polynomial.Monomials[i];
            var count = counts[i];
            var counterCount = counterCounts[i];
            var computedWeights =
                ComputeDistributionWeights(count, counterCount);
            if (Weighting == Weighting.ComputedBinary)
            {
                var maxIndex =
                    Array.IndexOf(computedWeights, computedWeights.Max());
                for (var j = 0; j < computedWeights.Length; j++)
                    computedWeights[j] = j == maxIndex ? 1.0f : 0.0f;
            }

            monomial.Weights = computedWeights;
        }

        var computedPolynomialWeights =
            ComputeDistributionWeights(counts[^1], counterCounts[^1]);
        if (Weighting == Weighting.ComputedBinary)
        {
            var maxIndex =
                Array.IndexOf(computedPolynomialWeights,
                    computedPolynomialWeights.Max());
            for (var j = 0; j < computedPolynomialWeights.Length; j++)
                computedPolynomialWeights[j] = j == maxIndex ? 1.0f : 0.0f;
        }

        Polynomial.Weights =
            computedPolynomialWeights;
    }

    private float[] ComputeDistributionWeights(int[] count, int[] counterCount)
    {
        var inDistribution = new float[count.Length];
        var outDistribution = new float[counterCount.Length];

        float sum = 0;
        for (var i = 0; i < count.Length; i++)
        {
            inDistribution[i] = count[i];
            sum += inDistribution[i];
        }

        if (sum == 0) sum = 1;
        for (var j = 0; j < inDistribution.Length; j++)
            inDistribution[j] /= sum;

        sum = 0;
        for (var i = 0; i < counterCount.Length; i++)
        {
            outDistribution[i] = counterCount[i];
            sum += outDistribution[i];
        }

        if (sum == 0) sum = 1;
        for (var j = 0; j < outDistribution.Length; j++)
            outDistribution[j] /= sum;

        var newWeights = new float[count.Length];
        for (var j = 0; j < newWeights.Length; j++)
            newWeights[j] = outDistribution[j] == 0
                ? inDistribution[j]
                : inDistribution[j] / outDistribution[j];

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
        return Polynomial.ToString() ?? string.Empty;
    }

    private IMonomial<TCategory> GetRandomMonomial()
    {
        return Polynomial.GetRandomMonomial();
    }

    public static IGenotype GenerateRandomGenotype<TCategory>(
        List<ILiteral<TCategory>> literals, Weighting weighting)
    {
        var literal =
            literals[ThreadSafeRandomNetCore.Shared.Next(literals.Count)];
        var monomial = new LogicGpMonomial<TCategory>([literal]);
        var polynomial = new LogicGpPolynomial<TCategory>([monomial]);
        return new LogicGpGenotype<TCategory>(polynomial, literals, weighting);
    }
}