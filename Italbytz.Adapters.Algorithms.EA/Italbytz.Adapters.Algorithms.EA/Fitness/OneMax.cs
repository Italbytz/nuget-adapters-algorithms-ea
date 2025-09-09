using System.Linq;
using Italbytz.EA.Individuals;
using Italbytz.EA.Searchspace;

namespace Italbytz.EA.Fitness;

public class OneMax : IStaticSingleObjectiveFitnessFunction
{
    public double[] Evaluate(IIndividual individual)
    {
        var result = ((BitStringGenotype)individual.Genotype).BitArray;
        var count = result.Cast<bool>().Count(bit => bit);
        return [count];
    }

    public int NumberOfObjectives { get; }
}