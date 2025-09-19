using System.Linq;
using Italbytz.EA.Individuals;
using Italbytz.EA.Searchspace;

namespace Italbytz.EA.Fitness;

public class OneMax : IFitnessFunction
{
    public IFitnessValue Evaluate(IIndividual individual)
    {
        var result = ((BitStringGenotype)individual.Genotype).BitArray;
        var count = result.Cast<bool>().Count(bit => bit);
        return new SingleFitnessValue(count);
    }

    public int NumberOfObjectives { get; }
}