using System.Diagnostics;

namespace Italbytz.EA.StoppingCriterion;

public class TimeStoppingCriterion(int limit = 60) : IStoppingCriterion
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    /// <inheritdoc />
    public bool IsMet()
    {
        return _stopwatch.Elapsed.TotalSeconds >= limit;
        //Console.WriteLine($"Stopping after {_stopwatch.Elapsed}");
    }
}