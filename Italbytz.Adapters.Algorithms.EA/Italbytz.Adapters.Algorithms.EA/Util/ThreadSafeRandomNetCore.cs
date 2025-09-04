using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Italbytz.EA.Util;

/// <summary>
///     Provides thread-safe random number generation for .NET Core.
/// </summary>
public class ThreadSafeRandomNetCore : Random
{
    private static int? _seed;
    [ThreadStatic] private static Random? _tRandom;

    /// <summary>
    ///     Gets or sets the seed for random number generation.
    /// </summary>
    public static int? Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            _tRandom =
                null; // Reset to ensure next access creates a new Random with the seed
        }
    }


    public static Random LocalRandom => _tRandom ?? Create();


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Random Create()
    {
        return _tRandom =
            _seed.HasValue ? new Random(_seed.Value) : new Random();
    }

    public override int Next()
    {
        var result = LocalRandom.Next();
        AssertInRange(result, 0, int.MaxValue);
        return result;
    }

    public override int Next(int maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxValue);

        var result = LocalRandom.Next(maxValue);
        AssertInRange(result, 0, maxValue);
        return result;
    }

    private static void ThrowMinMaxValueSwapped()
    {
        throw new ArgumentOutOfRangeException("minValue");
    }

    public override int Next(int minValue, int maxValue)
    {
        if (minValue > maxValue) ThrowMinMaxValueSwapped();

        var result = LocalRandom.Next(minValue, maxValue);
        AssertInRange(result, minValue, maxValue);
        return result;
    }

    public override long NextInt64()
    {
        var result = LocalRandom.NextInt64();
        AssertInRange(result, 0, long.MaxValue);
        return result;
    }

    public override long NextInt64(long maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxValue);

        var result = LocalRandom.NextInt64(maxValue);
        AssertInRange(result, 0, maxValue);
        return result;
    }

    public override long NextInt64(long minValue, long maxValue)
    {
        if (minValue > maxValue) ThrowMinMaxValueSwapped();

        var result = LocalRandom.NextInt64(minValue, maxValue);
        AssertInRange(result, minValue, maxValue);
        return result;
    }

    public override float NextSingle()
    {
        var result = LocalRandom.NextSingle();
        AssertInRange(result);
        return result;
    }

    public override double NextDouble()
    {
        var result = LocalRandom.NextDouble();
        AssertInRange(result);
        return result;
    }

    public override void NextBytes(byte[] buffer)
    {
        if (buffer is null)
            throw new ArgumentNullException(nameof(buffer));

        LocalRandom.NextBytes(buffer);
    }

    public override void NextBytes(Span<byte> buffer)
    {
        LocalRandom.NextBytes(buffer);
    }

    protected override double Sample()
    {
        throw new NotSupportedException();
    }

    private static void AssertInRange(long result, long minInclusive,
        long maxExclusive)
    {
        if (maxExclusive > minInclusive)
            Debug.Assert(result >= minInclusive && result < maxExclusive,
                $"Expected {minInclusive} <= {result} < {maxExclusive}");
        else
            Debug.Assert(result == minInclusive,
                $"Expected {minInclusive} == {result}");
    }

    private static void AssertInRange(double result)
    {
        Debug.Assert(result >= 0.0 && result < 1.0,
            $"Expected 0.0 <= {result} < 1.0");
    }
}