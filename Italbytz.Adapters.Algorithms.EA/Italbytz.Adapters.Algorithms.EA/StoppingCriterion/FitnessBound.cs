using System;

namespace Italbytz.EA.StoppingCriterion;

public class FitnessBound : IStoppingCriterion
{
    public bool IsMet()
    {
        Console.WriteLine(
            "FitnessBound stopping criterion is not implemented.");
        return false;
    }
}