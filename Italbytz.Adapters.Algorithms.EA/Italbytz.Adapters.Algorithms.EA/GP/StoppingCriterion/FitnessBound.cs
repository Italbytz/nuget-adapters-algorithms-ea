using System;
using Italbytz.EA.StoppingCriterion;

namespace Italbytz.EA.GP.StoppingCriterion;

public class FitnessBound : IStoppingCriterion
{
    public bool IsMet()
    {
        Console.WriteLine(
            "FitnessBound stopping criterion is not implemented.");
        return false;
    }
}