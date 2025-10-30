using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Italbytz.AI;
using Italbytz.EA.Crossover;
using Italbytz.EA.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Mutation;
using Italbytz.EA.SearchSpace;
using Italbytz.ML;

namespace Italbytz.EA.Searchspace;

public class WeightedPolynomialGenotype<TLiteral, TCategory> :
    IPredictingGenotype<TCategory>,
    ILogicGpMutable, ICrossable, IFreezable, IValidatableGenotype,
    ISaveable
    where TLiteral : ILiteral<TCategory>
{
    private readonly IList<TLiteral> _literals;

    [JsonInclude]
    public readonly WeightedPolynomial<TLiteral, TCategory> Polynomial;

    [JsonConstructor]
    public WeightedPolynomialGenotype(WeightedPolynomial<TLiteral,
            TCategory> polynomial,
        PredictionStrategy predictionStrategy) : this(
        polynomial, [],
        Weighting.Fixed, predictionStrategy)
    {
    }

    public WeightedPolynomialGenotype(
        WeightedPolynomial<TLiteral,
            TCategory> polynomial,
        IList<TLiteral> literals, Weighting weighting,
        PredictionStrategy predictionStrategy = PredictionStrategy.Max)
    {
        Polynomial = polynomial;
        _literals = literals;
        Weighting = weighting;
        PredictionStrategy = predictionStrategy;
    }

    public WeightedPolynomialGenotype(
        WeightedMonomial<TLiteral, TCategory> monomial,
        IList<TLiteral> literals, Weighting weighting) : this(
        new WeightedPolynomial<TLiteral,
            TCategory>([monomial]),
        literals,
        weighting)
    {
    }

    public WeightedPolynomialGenotype(TLiteral literal,
        List<TLiteral> literals, Weighting weighting) : this(
        new WeightedMonomial<TLiteral, TCategory>([literal]),
        literals,
        weighting)
    {
    }

    [JsonIgnore] public Weighting Weighting { get; set; } = Weighting.Fixed;

    public PredictionStrategy PredictionStrategy { get; set; } =
        PredictionStrategy.Max;

    public void CrossWith(ICrossable parentGenotype)
    {
        if (LatestKnownFitness != null) throw new InvalidOperationException();
        if (parentGenotype is not
            WeightedPolynomialGenotype<TLiteral, TCategory> parent)
            throw new InvalidOperationException(
                "Parent genotype is not of the same type");
        var monomial =
            (WeightedMonomial<TLiteral, TCategory>)parent.Polynomial
                .GetRandomMonomial().Clone();
        Polynomial.Monomials.Add(monomial);
    }

    public void Freeze()
    {
        Weighting = Weighting.Fixed;
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
        var monomial = new WeightedMonomial<TLiteral, TCategory>([literal]);
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
            (WeightedPolynomial<TLiteral,
                TCategory>)Polynomial.Clone();
        return new WeightedPolynomialGenotype<TLiteral, TCategory>(
            clonedPolynomial, _literals,
            Weighting);
    }

    [JsonIgnore] public IFitnessValue? LatestKnownFitness { get; set; }

    [JsonIgnore] public int Size => Polynomial.Size;

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
        /*Console.WriteLine(
            $"{string.Join(",", Polynomial.GetAllLiterals())} predicts {string.Join(",", result)} for {string.Join(",", features.Take(6))}");*/
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
        if (Polynomial.Weights == null || Weighting != Weighting.Fixed)
            ComputeWeights(features, labels);
        var results = new int[features.Length];
        Parallel.For(0, features.Length,
            i => { results[i] = PredictClass(features[i]); });
        /*for (var i = 0; i < results.Length; i++)
            if (results[i] == 0)
                Console.WriteLine($"Index {i + 2}: {results[i]}");*/

        return results;
    }

    public void Save(Stream stream)
    {
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = true
        };

        using var writer = new Utf8JsonWriter(stream,
            new JsonWriterOptions { Indented = true });
        JsonSerializer.Serialize(writer, this, GetType(), options);
        writer.Flush();
    }

    [JsonIgnore] public IFitnessValue? TrainingFitness { get; set; }

    [JsonIgnore] public IFitnessValue? ValidationFitness { get; set; }

    public string LiteralSignature()
    {
        var literals = Polynomial.GetAllLiterals();
        literals.ToList().Sort();
        return string.Join(" ",
            literals.Select(literal => "x")); //ToDo: literal.Label));
    }

    public void ComputeWeights(TCategory[][] features, int[] labels)
    {
        if (Weighting == Weighting.Fixed && Polynomial.Weights != null) return;
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
            if (Weighting is Weighting.ComputedBinary or Weighting.Fixed)
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
        if (Weighting is Weighting.ComputedBinary or Weighting.Fixed)
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

    private WeightedMonomial<TLiteral, TCategory> GetRandomMonomial()
    {
        return Polynomial.GetRandomMonomial();
    }

    public static IGenotype GenerateRandomGenotype(
        IList<TLiteral> literals, Weighting weighting)
    {
        var literal =
            literals[ThreadSafeRandomNetCore.Shared.Next(literals.Count)];
        var monomial = new WeightedMonomial<TLiteral, TCategory>([literal]);
        var polynomial =
            new WeightedPolynomial<TLiteral, TCategory>([monomial]);
        return new WeightedPolynomialGenotype<TLiteral, TCategory>(
            polynomial, literals,
            weighting);
    }
}